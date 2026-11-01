using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CityPress.Core;
using Colossal.Serialization.Entities;
using Colossal.UI.Binding;
using Game;
using Game.Simulation;
using Game.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CityPress.Systems {
 public sealed partial class NewsSystem : UISystemBase, IDefaultSerializable {
  internal static NewsSystem Active;
  SaveState state=new SaveState(); Preferences global;
  NewsService service; CancellationTokenSource lifetime=new CancellationTokenSource();
  Task<ArchiveResult> pending,reading;Task<string[]> listing;Task journal,opening;
  string pendingId,pendingDay;bool pendingPopup,pendingExportOnly;int epoch,pendingEpoch,readingEpoch,readingRequest,paperRequest,listingEpoch,journalEpoch,openingEpoch;
  ValueBinding<string> binding,clockBinding;Newspaper paper;int popup;string statusKey="CityPress.Status.Ready";object[] statusArgs=Array.Empty<object>();bool dirty,uiDirty=true,archiveRefresh=true,keyConfigured,lastReady;DateTime nextFlush,nextUi,nextFullUi;
  ExportStore archives;JournalStore journals;string[] issueDays=Array.Empty<string>();string paperFolder,lastDay,lastClock,lastCoverage;
  readonly Queue<DayLog> queue=new Queue<DayLog>();
  string city="城市";
  bool loaded;
  internal int SessionEpoch=>epoch;
  Preferences Effective=>state.Override??global;
  string Config=>Path.Combine(Storage.Root,"settings.json");
  string NewspaperName=>string.IsNullOrWhiteSpace(Effective.CustomName)?city+I18n.Say("日报"," Daily"):Effective.CustomName.Trim();
  string ExportFolder=>paperFolder??ExportPaths.Folder(Effective.ExportDirectory,NewspaperName,state.Id);
  void Changed(){dirty=true;uiDirty=true;}
  void Status(string key,params object[] args){statusKey=key;statusArgs=args??Array.Empty<object>();uiDirty=true;nextFullUi=default;}
  public override GameMode gameMode=>GameMode.Game;
  protected override void OnCreate() {
   base.OnCreate();Active=this;service=new NewsService();archives=new ExportStore();journals=new JournalStore();
   try{I18n.ActiveLocale=Game.SceneFlow.GameManager.instance.localizationManager.activeLocaleId;}catch{I18n.ActiveLocale="zh-HANS";}
   service.Warn=message=>{try{Mod.Log.Warn("CityPress 编辑部："+message);}catch{}};
   try{global=Storage.Read<Preferences>(Config);}catch{global=new Preferences();}
   if(EditorialPolicy.Upgrade(global))Storage.Write(Config,JsonConvert.SerializeObject(global));
   try{ExportPaths.Root(global.ExportDirectory);}catch{global.ExportDirectory="";}
   ApiOptions.Repair(global);keyConfigured=ApiKeys.Exists(global.ApiEndpoint);
   binding=new ValueBinding<string>("CityPress","state","{}");AddBinding(binding);
   clockBinding=new ValueBinding<string>("CityPress","clock","");AddBinding(clockBinding);
   AddBinding(new TriggerBinding("CityPress","generate",()=>Start(Current(),false)));
   AddBinding(new TriggerBinding<string>("CityPress","generateDay",day=>{var log=state.Days.FirstOrDefault(x=>x.Day==day);if(log!=null)Start(log,false);}));
   AddBinding(new TriggerBinding<string>("CityPress","openIssue",OpenIssue));
   AddBinding(new TriggerBinding<string>("CityPress","configure",Configure));
   AddBinding(new TriggerBinding("CityPress","refreshArchive",()=>archiveRefresh=true));
   AddBinding(new TriggerBinding("CityPress","openExportFolder",OpenExportFolder));
   AddBinding(new TriggerBinding("CityPress","exportIssue",ExportCurrent));
  }
  public void Record(string kind,string detail) {
   if(!EditorialInput.IsCandidate(kind,detail))return;
   if(!TryGetGameTime(out var now))return;
   Rollover(now.ToString("yyyy-MM-dd"));
   Aggregator.Add(state.Days.Last(),kind,detail,now.ToString("HH:mm"));Changed();
  }
  internal bool TryGetGameTime(out DateTime now) {
   now=default;
   if(!loaded)return false;
   if(Game.SceneFlow.GameManager.instance.gameMode!=GameMode.Game)return false;
   var entity=World.GetOrCreateSystemManaged<CitySystem>().City;
   if(entity==Unity.Entities.Entity.Null||!EntityManager.Exists(entity))return false;
   now=World.GetOrCreateSystemManaged<TimeSystem>().GetCurrentDateTime();
   return GameCalendar.IsReady(now);
  }
  DayLog Current() {
   if(!TryGetGameTime(out var now))return null;
   var day=now.ToString("yyyy-MM-dd");
   Rollover(day);return state.Days.Last();
  }
  internal void Sample(string key,string label,string unit,double value) {
   if(!TryGetGameTime(out var now))return;
   Rollover(now.ToString("yyyy-MM-dd"));
   var log=state.Days.Last();Aggregator.Sample(log,key,label,unit,value,now.ToString("HH:mm:ss"));Milestones.Observe(state,log,key,value,now.ToString("HH:mm:ss"));Changed();
  }
  internal void ObserveTransit(TransitRecord value){if(!TryGetGameTime(out var now))return;Rollover(now.ToString("yyyy-MM-dd"));value.Time=now.ToString("HH:mm:ss");TransitFacts.Store(state.Days.Last(),value);Changed();}
  protected override void OnGamePreload(Purpose purpose,GameMode mode){base.OnGamePreload(purpose,mode);loaded=false;Reset();}
  protected override void OnGameLoadingComplete(Purpose purpose,GameMode mode){base.OnGameLoadingComplete(purpose,mode);loaded=mode==GameMode.Game;archiveRefresh=true;keyConfigured=ApiKeys.Exists(Effective.ApiEndpoint);}
  void Rollover(string day) {
   if(!GameCalendar.IsValidDay(day))return;
   if(state.OmittedEmptyDays?.Remove(day)==true)Changed();
   if(state.Days.LastOrDefault()?.Day==day)return;
   var previous=state.Days.LastOrDefault();
   // Completed day objects are immutable; avoid serializing them just to queue.
   if(previous!=null && Effective.AutoGenerate && string.CompareOrdinal(day,previous.Day)>0)queue.Enqueue(previous);
   // Time reversal starts a new branch of that day's facts.
   state.Days.RemoveAll(x=>string.CompareOrdinal(x.Day,day)>=0);
   state.Days.Add(new DayLog{Day=day});Changed();
  }
  void Configure(string raw) {
   try {
    var obj=JObject.Parse(raw);string scope=(string)obj["scope"];
    if(scope!="inherit"&&scope!="global"&&scope!="save")throw new ArgumentException();
    var prefs=scope=="inherit"?global:obj["preferences"].ToObject<Preferences>();
    if(scope!="inherit"){
     if(prefs==null||prefs.CustomName?.Length>60||prefs.Editorial?.Length>6000||string.IsNullOrWhiteSpace(prefs.Model)||prefs.Model.Length>100)throw new ArgumentException();
     ExportPaths.Root(prefs.ExportDirectory);ApiOptions.Validate(prefs);
    }
    var newKey=(string)obj["apiKey"];if(!string.IsNullOrWhiteSpace(newKey)){
     if(scope=="inherit")throw new ArgumentException("恢复继承时不更改服务密钥。");
     if(newKey.Length>4096||newKey.Any(char.IsControl))throw new ArgumentException("密钥格式无效");ApiKeys.Save(prefs.ApiEndpoint,newKey);
    }
    if(scope=="inherit")state.Override=null;else if(scope=="global"){Storage.Write(Config,JsonConvert.SerializeObject(prefs));global=prefs;state.Override=null;}else state.Override=prefs;
    keyConfigured=ApiKeys.Exists(Effective.ApiEndpoint);
    Changed();Status("CityPress.Status.SettingsSaved");
   }catch(ArgumentException){Status("CityPress.Status.SettingsInvalid");}catch{Status("CityPress.Status.SettingsFailed");}
  }
  void Start(DayLog day,bool auto) {
   try{ApiOptions.Validate(Effective);}catch(ArgumentException){Status("CityPress.Status.ConfigInvalid");return;}
   if(!TryGetGameTime(out _)||day==null||!GameCalendar.IsValidDay(day.Day)){Status("CityPress.Status.TimeNotReady");return;}
   if(pending!=null){Status("CityPress.Status.Busy");return;}
   pendingId=state.Id;pendingDay=day.Day;pendingPopup=auto;pendingEpoch=epoch;pendingExportOnly=false;paperRequest++;Status("CityPress.Status.Generating");
   var snapshot=JournalSnapshot.CopyDay(day);
   var prefs=JsonConvert.DeserializeObject<Preferences>(JsonConvert.SerializeObject(Effective));
   var saveId=state.Id;var currentCity=city;var token=lifetime.Token;
   pending=Task.Run(async()=>{var result=await service.Generate(saveId,currentCity,snapshot,prefs,prefs.RequireApiKey?ApiKeys.Read(prefs.ApiEndpoint):"",token).ConfigureAwait(false);token.ThrowIfCancellationRequested();return archives.Save(saveId,result,prefs.ExportDirectory,token);},token);
  }
  string Issues=>Path.Combine(Storage.Root,"cities",state.Id,"issues");
  void OpenIssue(string day){
   if(!GameCalendar.IsValidDay(day)||pending!=null)return;
   if(!GameCalendar.IsVisibleIssue(state,day,TryGetGameTime(out var now)?now.ToString("yyyy-MM-dd"):"")){Status("CityPress.Status.NoHistory");return;}
   var saveId=state.Id;var root=Effective.ExportDirectory;readingEpoch=epoch;readingRequest=++paperRequest;Status("CityPress.Status.Reading");
   reading=Task.Run(()=>archives.Read(saveId,day,root));reading.ContinueWith(task=>{var ignored=task.Exception;},TaskContinuationOptions.OnlyOnFaulted);
  }
  void ExportCurrent(){
   if(paper==null){Status("CityPress.Status.NoPaper");return;}if(pending!=null){Status("CityPress.Status.Busy");return;}
   var copy=JsonConvert.DeserializeObject<Newspaper>(JsonConvert.SerializeObject(paper));var saveId=state.Id;var root=Effective.ExportDirectory;var token=lifetime.Token;
   pendingId=saveId;pendingDay=copy.Day;pendingEpoch=epoch;pendingPopup=false;pendingExportOnly=true;paperRequest++;Status("CityPress.Status.Exporting");pending=Task.Run(()=>archives.Save(saveId,copy,root,token),token);
  }
  void OpenExportFolder(){
   if(opening!=null)return;string folder;try{folder=ExportFolder;}catch{Status("CityPress.Status.ExportFolderInvalid");return;}
   openingEpoch=epoch;opening=Task.Run(()=>{Directory.CreateDirectory(folder);Process.Start(new ProcessStartInfo{FileName=folder,UseShellExecute=true});});
  }
  protected override void OnUpdate() {
   base.OnUpdate();
   if(DateTime.UtcNow<nextUi)return;nextUi=DateTime.UtcNow.AddMilliseconds(250);
   // The city entity is not a named prefab. Rendering its label passes a null
   // localization key to NameSystem (and localization mods) during loading.
   // Read the authoritative city configuration instead; this is also safe
   // before the city entity and its prefab data have finished loading.
   var currentCity=World.GetOrCreateSystemManaged<Game.City.CityConfigurationSystem>().cityName;if(string.IsNullOrWhiteSpace(currentCity))currentCity=I18n.Say("城市","City");
   if(currentCity!=city){city=currentCity;uiDirty=true;}
   bool timeReady=TryGetGameTime(out var now);string day=timeReady?now.ToString("yyyy-MM-dd"):"",clock=timeReady?now.ToString("yyyy-MM-dd HH:mm:ss"):"";
   if(clock!=lastClock){clockBinding.Update(timeReady?clock:"!");lastClock=clock;}
   if(timeReady!=lastReady){lastReady=timeReady;uiDirty=true;}
   if(timeReady&&day!=lastDay){if(GameCalendar.PruneEmptyHistory(state,day)>0)Changed();Rollover(day);lastDay=day;uiDirty=true;}
   if(pending?.IsCompleted==true){bool current=pendingId==state.Id&&pendingEpoch==epoch;
     try{var result=pending.GetAwaiter().GetResult();if(current){paper=result.Paper;paperFolder=result.Folder;archiveRefresh=true;if(pendingPopup)popup++;int pages=paper.Pages?.Count??1;int dropped=paper.DroppedUnsafeArticles;int score=paper.QualityScore;
      if(pendingExportOnly)Status("CityPress.Status.ExportDone");
      else if(paper.LocalOnly)Status("CityPress.Status.LocalOnly");
      else if(paper.LocalCache)Status("CityPress.Status.CacheReused");
      else if(dropped>0&&score<100)Status("CityPress.Status.GeneratedWithheldQuality",pages,dropped,score);
      else if(dropped>0)Status("CityPress.Status.GeneratedWithheld",pages,dropped);
      else if(score<100)Status("CityPress.Status.GeneratedQuality",pages,score);
      else Status("CityPress.Status.Generated",pages);
     }}
     catch(OperationCanceledException){}catch(Exception error){if(current){if(error is LocalizedException localized)Status(localized.Key,localized.Args);else Status("CityPress.Status.GenerationFailed",error.GetType().Name);Mod.Log.Warn("新闻生成或导出失败："+error.GetType().Name+"\n"+error.StackTrace);}}finally{pending=null;uiDirty=true;}
   }
   if(reading?.IsCompleted==true){bool current=readingEpoch==epoch&&readingRequest==paperRequest;
     try{var result=reading.GetAwaiter().GetResult();if(current){paper=result.Paper;paperFolder=result.Folder;Status("CityPress.Status.Opened");}}catch{if(current)Status("CityPress.Status.ReadFailed");}finally{reading=null;}
   }
   if(listing?.IsCompleted==true){try{var result=listing.GetAwaiter().GetResult();if(listingEpoch==epoch){issueDays=result;uiDirty=true;}}catch{if(listingEpoch==epoch)Status("CityPress.Status.ArchiveListFailed");}finally{listing=null;}}
   if(archiveRefresh&&listing==null&&timeReady){archiveRefresh=false;listingEpoch=epoch;var saveId=state.Id;listing=Task.Run(()=>archives.List(saveId));}
   if(opening?.IsCompleted==true){try{opening.GetAwaiter().GetResult();}catch{if(openingEpoch==epoch)Status("CityPress.Status.FolderOpenFailed");}finally{opening=null;}}
   if(journal?.IsCompleted==true){try{journal.GetAwaiter().GetResult();}catch(OperationCanceledException){}catch{if(journalEpoch==epoch){dirty=true;Status("CityPress.Status.JournalFailed");}}finally{journal=null;}}
   if(dirty&&journal==null&&timeReady&&DateTime.UtcNow>=nextFlush){
    var snapshot=JournalSnapshot.Capture(state);var token=lifetime.Token;journalEpoch=epoch;dirty=false;nextFlush=DateTime.UtcNow.AddSeconds(10);
    journal=Task.Run(()=>journals.Write(snapshot,token),token);
   }
   if(timeReady&&pending==null&&queue.Count>0)Start(queue.Dequeue(),true);
   if(lastCoverage!=CityObserver.Report){lastCoverage=CityObserver.Report;uiDirty=true;}
   if(!uiDirty||DateTime.UtcNow<nextFullUi)return;uiDirty=false;nextFullUi=DateTime.UtcNow.AddSeconds(1);
   binding.Update(JsonConvert.SerializeObject(new {city,day,gameTime=clock,timeReady,preferences=Effective,scope=state.Override==null?"global":"save",busy=pending!=null,status=new{key=statusKey,args=statusArgs},popup,paper,
    days=state.Days.Where(x=>GameCalendar.IsValidDay(x.Day)).Select(x=>new{x.Day,count=Aggregator.Count(x),metrics=x.Metrics?.Count??0,breakthroughs=x.Breakthroughs?.Count??0,transit=x.Transit?.Count??0}),issues=issueDays.Where(value=>GameCalendar.IsVisibleIssue(state,value,day)),
    coverage=ActionHooks.Coverage.Concat(new[]{I18n.Say("管理操作入口："+GameplayBindings.Count+" 个；未接入："+GameplayBindings.Failed,"Management actions: "+GameplayBindings.Count+" registered; "+GameplayBindings.Failed+" unavailable"),CityObserver.Report}),keyConfigured,exportDefault=ExportPaths.DefaultDirectory,exportFolder=ExportFolder}));
  }
  public void Serialize<TWriter>(TWriter writer) where TWriter:IWriter {writer.Write(1);writer.Write(JsonConvert.SerializeObject(state));}
  public void Deserialize<TReader>(TReader reader) where TReader:IReader {
   Reset();reader.Read(out int version);reader.Read(out string json);
   if(version!=1)throw new InvalidDataException("不支持的 CityPress 存档版本");
   state=JsonConvert.DeserializeObject<SaveState>(json)??new SaveState();
   if(string.IsNullOrWhiteSpace(state.Id))state.Id=Guid.NewGuid().ToString("N");
   if(GameCalendar.RemoveInvalidDays(state)>0)dirty=true;
   if(EditorialPolicy.Upgrade(state.Override))dirty=true;
   if(state.Override!=null)ApiOptions.Repair(state.Override);
   if(state.Override!=null)try{ExportPaths.Root(state.Override.ExportDirectory);}catch{state.Override.ExportDirectory="";}
   Changed();
  }
  public void SetDefaults(Context context){Reset();state=new SaveState();Changed();}
  void Reset(){epoch++;lifetime.Cancel();lifetime.Dispose();lifetime=new CancellationTokenSource();queue.Clear();paper=null;paperFolder=null;popup=0;dirty=false;uiDirty=true;archiveRefresh=true;issueDays=Array.Empty<string>();lastDay=null;lastCoverage=null;paperRequest++;nextFullUi=default;statusKey="CityPress.Status.Ready";statusArgs=Array.Empty<object>();}
  protected override void OnDestroy(){lifetime.Cancel();service.Dispose();lifetime.Dispose();if(Active==this)Active=null;base.OnDestroy();}
 }
}
