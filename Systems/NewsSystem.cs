using System;
using System.Collections.Generic;
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
  Task<Newspaper> pending; string pendingId,pendingDay; bool pendingPopup; int epoch,pendingEpoch;
  ValueBinding<string> binding; Newspaper paper; int popup; string status="就绪"; bool dirty; DateTime nextFlush,nextUi;
  readonly Queue<DayLog> queue=new Queue<DayLog>();
  string key="", city="城市";
  bool loaded;
  internal int SessionEpoch=>epoch;
  Preferences Effective=>state.Override??global;
  string Config=>Path.Combine(Storage.Root,"settings.json");
  public override GameMode gameMode=>GameMode.Game;
  protected override void OnCreate() {
   base.OnCreate(); Active=this;service=new NewsService();global=Storage.Read<Preferences>(Config);
   if(EditorialPolicy.Upgrade(global))Storage.Write(Config,JsonConvert.SerializeObject(global));
   binding=new ValueBinding<string>("CityPress","state","{}");AddBinding(binding);
   AddBinding(new TriggerBinding("CityPress","generate",()=>Start(Current(),false)));
   AddBinding(new TriggerBinding<string>("CityPress","generateDay",day=>{var log=state.Days.FirstOrDefault(x=>x.Day==day);if(log!=null)Start(log,false);}));
   AddBinding(new TriggerBinding<string>("CityPress","openIssue",OpenIssue));
   AddBinding(new TriggerBinding<string>("CityPress","configure",Configure));
  }
  public void Record(string kind,string detail) {
   if(!EditorialInput.IsCandidate(kind,detail))return;
   if(!TryGetGameTime(out var now))return;
   Rollover(now.ToString("yyyy-MM-dd"));
   Aggregator.Add(state.Days.Last(),kind,detail,now.ToString("HH:mm"));dirty=true;
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
   Aggregator.Sample(state.Days.Last(),key,label,unit,value,now.ToString("HH:mm:ss"));dirty=true;
  }
  protected override void OnGamePreload(Purpose purpose,GameMode mode){base.OnGamePreload(purpose,mode);loaded=false;Reset();}
  protected override void OnGameLoadingComplete(Purpose purpose,GameMode mode){base.OnGameLoadingComplete(purpose,mode);loaded=mode==GameMode.Game;if(loaded)try{GameplayBindings.Scan(this);}catch{Mod.Log.Warn("管理操作入口尚未就绪，将自动重试。");}}
  void Rollover(string day) {
   if(!GameCalendar.IsValidDay(day))return;
   if(state.Days.LastOrDefault()?.Day==day)return;
   var previous=state.Days.LastOrDefault();
   if(previous!=null && Effective.AutoGenerate && string.CompareOrdinal(day,previous.Day)>0)queue.Enqueue(JsonConvert.DeserializeObject<DayLog>(JsonConvert.SerializeObject(previous)));
   // Time reversal starts a new branch of that day's facts.
   state.Days.RemoveAll(x=>string.CompareOrdinal(x.Day,day)>=0);
   state.Days.Add(new DayLog{Day=day});dirty=true;
  }
  void Configure(string raw) {
   try {
    var obj=JObject.Parse(raw);string scope=(string)obj["scope"];
    if(scope=="inherit")state.Override=null;
    else {
     var prefs=obj["preferences"].ToObject<Preferences>();
     if(prefs==null||prefs.CustomName?.Length>60||prefs.Editorial?.Length>6000||string.IsNullOrWhiteSpace(prefs.Model)||prefs.Model.Length>100)throw new Exception();
     if(scope=="global"){global=prefs;Storage.Write(Config,JsonConvert.SerializeObject(global));state.Override=null;}else state.Override=prefs;
    }
    var newKey=(string)obj["apiKey"];if(!string.IsNullOrWhiteSpace(newKey)){
     key=newKey.Trim();
     var protectedBytes=System.Security.Cryptography.ProtectedData.Protect(System.Text.Encoding.UTF8.GetBytes(key),null,System.Security.Cryptography.DataProtectionScope.CurrentUser);
     Directory.CreateDirectory(Storage.Root);File.WriteAllBytes(Path.Combine(Storage.Root,"key.dpapi"),protectedBytes);
    }
    dirty=true;status="设置已保存";
   }catch {status="设置无效，未保存";}
  }
  void Start(DayLog day,bool auto) {
   if(!TryGetGameTime(out _)||day==null||!GameCalendar.IsValidDay(day.Day)){status="城市时间尚未就绪，请等待城市加载完成。";return;}
   if(pending!=null){status="正在排版，请稍候";return;}
   if(string.IsNullOrWhiteSpace(key))try {
    key=System.Text.Encoding.UTF8.GetString(System.Security.Cryptography.ProtectedData.Unprotect(File.ReadAllBytes(Path.Combine(Storage.Root,"key.dpapi")),null,System.Security.Cryptography.DataProtectionScope.CurrentUser));
   }catch {key=Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY")??"";}
   pendingId=state.Id;pendingDay=day.Day;pendingPopup=auto;pendingEpoch=epoch;status="编辑部正在整理新闻…";
   var snapshot=JsonConvert.DeserializeObject<DayLog>(JsonConvert.SerializeObject(day));
   var prefs=JsonConvert.DeserializeObject<Preferences>(JsonConvert.SerializeObject(Effective));
   pending=service.Generate(state.Id,city,snapshot,prefs,key,lifetime.Token);
  }
  string Issues=>Path.Combine(Storage.Root,"cities",state.Id,"issues");
  void OpenIssue(string day){ if(!GameCalendar.IsValidDay(day))return;try{paper=Storage.Read<Newspaper>(Path.Combine(Issues,day+".json"));}catch{status="存档读取失败";} }
  protected override void OnUpdate() {
   base.OnUpdate();
   if(DateTime.UtcNow<nextUi)return;nextUi=DateTime.UtcNow.AddMilliseconds(250);
   // The city entity is not a named prefab. Rendering its label passes a null
   // localization key to NameSystem (and localization mods) during loading.
   // Read the authoritative city configuration instead; this is also safe
   // before the city entity and its prefab data have finished loading.
   city=World.GetOrCreateSystemManaged<Game.City.CityConfigurationSystem>().cityName;
   if(string.IsNullOrWhiteSpace(city))city="城市";
   bool timeReady=TryGetGameTime(out var now);
   if(timeReady)Rollover(now.ToString("yyyy-MM-dd"));
   if(pending?.IsCompleted==true){
    try {var result=pending.GetAwaiter().GetResult();
     if(pendingId==state.Id && pendingEpoch==epoch){
      var dir=Path.Combine(Storage.Root,"cities",pendingId,"issues");
      // Archive before publishing; a failed archive remains visible as a failure.
      DocxExporter.Save(Path.Combine(dir,pendingDay+".docx"),result);
      Storage.Write(Path.Combine(dir,pendingDay+".json"),JsonConvert.SerializeObject(result));paper=result;if(pendingPopup)popup++;status=result.LocalOnly?"本期暂无可刊发事件，已保存简讯；未调用 API。":result.LocalCache?"已复用本地成稿":"新闻已生成并保存";
     }
    }catch(OperationCanceledException){}catch(Exception e){status=e is InvalidOperationException?e.Message:"生成或本地保存失败（"+e.GetType().Name+"），请重试；编辑部保留已生成的缓存。";Mod.Log.Warn("新闻生成或保存失败："+e.GetType().Name+"\n"+e.StackTrace);}
    finally{pending=null;}
   }
   if(timeReady&&pending==null&&queue.Count>0)Start(queue.Dequeue(),true);
   if(dirty && DateTime.UtcNow>=nextFlush){
    try{Storage.Write(Path.Combine(Storage.Root,"cities",state.Id,"journal.json"),JsonConvert.SerializeObject(state));dirty=false;}catch{status="操作日志保存失败";}
    nextFlush=DateTime.UtcNow.AddSeconds(5);
   }
   binding.Update(JsonConvert.SerializeObject(new {city,day=timeReady?now.ToString("yyyy-MM-dd"):"",gameTime=timeReady?now.ToString("yyyy-MM-dd HH:mm:ss"):"",timeReady,preferences=Effective,scope=state.Override==null?"global":"save",busy=pending!=null,status,popup,paper,days=state.Days.Where(x=>GameCalendar.IsValidDay(x.Day)).Select(x=>new{x.Day,count=x.Actions.Sum(a=>a.Count)}),issues=Directory.Exists(Issues)?Directory.GetFiles(Issues,"*.json").Select(Path.GetFileNameWithoutExtension).Where(GameCalendar.IsValidDay).OrderByDescending(x=>x).ToArray():Array.Empty<string>(),coverage=ActionHooks.Coverage.Concat(new[]{"管理操作入口："+GameplayBindings.Count+" 个；未接入："+GameplayBindings.Failed,CityObserver.Report}),keyConfigured=!string.IsNullOrWhiteSpace(key)||File.Exists(Path.Combine(Storage.Root,"key.dpapi"))}));
  }
  public void Serialize<TWriter>(TWriter writer) where TWriter:IWriter {writer.Write(1);writer.Write(JsonConvert.SerializeObject(state));}
  public void Deserialize<TReader>(TReader reader) where TReader:IReader {
   Reset();reader.Read(out int version);reader.Read(out string json);
   if(version!=1)throw new InvalidDataException("不支持的 CityPress 存档版本");
   state=JsonConvert.DeserializeObject<SaveState>(json)??new SaveState();
   if(GameCalendar.RemoveInvalidDays(state)>0)dirty=true;
   if(EditorialPolicy.Upgrade(state.Override))dirty=true;
  }
  public void SetDefaults(Context context){Reset();state=new SaveState();}
  void Reset(){epoch++;lifetime.Cancel();lifetime.Dispose();lifetime=new CancellationTokenSource();queue.Clear();paper=null;popup=0;dirty=false;}
  protected override void OnDestroy(){lifetime.Cancel();service.Dispose();lifetime.Dispose();if(Active==this)Active=null;base.OnDestroy();}
 }
}
