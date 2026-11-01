using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Linq;
using System.Xml.Linq;
using CityPress.Core;
using Newtonsoft.Json;

class Fake : HttpMessageHandler {
 public int Calls;public bool Invalid;public string Last;
 protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req,CancellationToken ct){Calls++;Last=await req.Content.ReadAsStringAsync();return new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonConvert.SerializeObject(new { choices=new[]{new{finish_reason="stop",message=new{content=Invalid?"{}":JsonConvert.SerializeObject(new{headline="城区建设持续推进",deck="道路工程与公共服务的新进展。",articles=new[]{new{genre="消息",section="城市建设",title="道路建设纪事",body="城区多项道路工程同步推进，施工安排涉及道路与步行通道。常住人口与市库资金余额等指标出现新的变化，城市公共服务与交通衔接也需要相应安排。"},new{genre="评论",section="城市评论",title="公共投入应回应城市需求",body="道路工程应统筹步行与公共交通衔接，让建设安排有明确的服务目标。\n公共投入的价值取决于服务是否真正回应城市需求：常住人口与市库资金余额的变化说明，城市建设不能只看工程数量，还要看后续维护与公共服务的承受能力。\n从长期看，交通与民生的衔接是衡量工程成效的关键。后续安排应结合站点可达性、步行环境和预算取舍，避免把施工本身当成已经完成的公共服务。\n这类讨论必须区分已确认的事实与有条件的影响：资金余额只是存量数据，不能直接说明服务已经改善；人口变化也需要结合设施分布来判断。城市管理的选择因此更应谨慎，把公共利益的权衡放在工程进度之前，并关注长期维护带来的财政风险。\n只有当实际运营、服务覆盖与预算安排相互匹配时，工程投入才可能转化为市民能够感受到的便利，这一判断仍取决于后续可核实的数据。"},new{genre="评论",section="短评",title="社区空间应方便日常步行",body="公共服务的可达性取决于设施与居住区之间的实际联系。建设安排宜把步行通道、过街条件与公共交通接驳放在一起考虑，让市民能够在日常出行中方便使用服务。\n这一选择也涉及长期维护的取舍。新增设施之后，通行环境是否便利、连接是否连续，都需要结合具体地点评估。以公共利益为出发点安排后续改善，比单纯追求设施数量更有利于形成完整的社区服务网络。"}}})}}},usage=new{prompt_cache_hit_tokens=128,prompt_cache_miss_tokens=20}}))};}
}
class Program {
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);Console.WriteLine("PASS "+message);}
 static async Task Main(string[] args){
  var projectRoot=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../"));
  if(args.Length==2&&args[0]=="--audit-input") {
   var saved=Storage.Read<SaveState>(args[1]);
   foreach(var day in saved.Days.Where(d=>GameCalendar.IsValidDay(d.Day)))Console.WriteLine(day.Day+": recorded="+day.Actions.Count+", eligible="+EditorialInput.Select(day).Count);
   return;
  }
  if(args.Length==3&&args[0]=="--export-cache") {
   var project=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../"));
   var assembly=System.Reflection.Assembly.Load(File.ReadAllBytes(Path.Combine(project,"bin/Release/net48/CityPress.dll")));
   var paperType=assembly.GetType("CityPress.Core.Newspaper");
   var newspaper=JsonConvert.DeserializeObject(File.ReadAllText(args[1]),paperType);
   assembly.GetType("CityPress.Core.DocxExporter").GetMethod("Save").Invoke(null,new object[]{Path.GetFullPath(args[2]),newspaper});
   Console.WriteLine("Exported original cached newspaper: "+Path.GetFullPath(args[2]));
   return;
  }
  var temp=Path.Combine(Path.GetTempPath(),"CityPress-tests-"+Guid.NewGuid().ToString("N"));
  bool coreOnly=args.Contains("--core-only");
  var handler=new Fake();using var service=new NewsService(handler,temp);
  try {
   await FeatureTests.Run(Path.Combine(temp,"features"));
   await EditionTests.Run(temp,projectRoot);
   await ApiLayoutTests.Run(temp,projectRoot);
   await EditorialTests.Run(temp);
   MaterialTests.Run();
   LocalizationTests.Run(projectRoot);
   var log=new DayLog{Day="2026-10-31"};Aggregator.Add(log,"道路","公路","10:00");Aggregator.Add(log,"道路","公路","11:00");
   Check(log.Actions.Count==1&&log.Actions[0].Count==2&&log.Actions[0].Last=="11:00","重复操作合并且保留次数和时间");
   var prefs=new Preferences();Check(!prefs.AutoGenerate,"默认手动生成");
   var noisyDay=new DayLog{Day="2027-01-12"};
   noisyDay.Actions.Add(new ActionRecord{Kind="NameSystem.SetCustomName",Detail="Entity(1261388:1) | 结束上客 10:35",Count=9999});
   noisyDay.Actions.Add(new ActionRecord{Kind="PoliciesUISystem.SetPolicy",Detail="Entity(2207084:1) | Entity(159:1) | True | 4"});
   var filterHandler=new Fake();using(var filterService=new NewsService(filterHandler,Path.Combine(temp,"filter"))) {
    var blank=await filterService.Generate("noise","汐澜",noisyDay,prefs,"",CancellationToken.None);
    Check(blank.LocalOnly&&blank.Articles.Count==0&&filterHandler.Calls==0,"纯运行状态与不明政策不凑新闻且不调用 API");
    noisyDay.Actions.Add(new ActionRecord{Kind="工具提交",Detail="Object Tool / CN_Hospital01"});
    var intent=await filterService.Generate("intent","汐澜",noisyDay,prefs,"",CancellationToken.None);Check(intent.LocalOnly&&filterHandler.Calls==0,"仅有医院放置意图不冒充建设事实");
    noisyDay.Actions.Add(new ActionRecord{Kind="城市实况 · 建筑进入施工",Detail="CN_Hospital01"});
    await filterService.Generate("noise","汐澜",noisyDay,prefs,"test",CancellationToken.None);
    var request=Newtonsoft.Json.Linq.JObject.Parse(filterHandler.Last);var input=Newtonsoft.Json.Linq.JObject.Parse((string)request["messages"][1]["content"]);
    Check(filterHandler.Calls==1&&input["actions"].Count()==1&&input["actions"][0]["Detail"].ToString().Contains("医疗设施")&&!filterHandler.Last.Contains("结束上客")&&!filterHandler.Last.Contains("2207084"),"高频噪声在发送前剔除，医院建设记录保留");
   }
   Check(EditorialInput.IsCandidate("市政建设","公交站上下客区无障碍改造")&&!EditorialInput.IsCandidate("工具提交","Object Tool / 无预制体"),"按记录来源过滤，不误伤涉及上下客区的实际建设");
   Check(!EditorialInput.IsCandidate("PoliciesUISystem.SetPolicy","Entity.Null | Entity(159:1) | True | 7"),"城市级空实体政策编号同样排除");
   Check(!EditorialInput.IsCandidate("玩家操作 · PoliciesUISystem.SetPolicy","target=Entity(1:1) | policy=Entity(2:1) | active=True | adjustment=4")&&EditorialInput.IsCandidate("玩家操作 · PoliciesUISystem.SetPolicy","target=东城区 | policy=重型车辆禁行 | active=True | adjustment=4"),"新政策采集必须解析措施名称，纯编号不能凑稿");
   Check(CapturePolicy.IsGameplayTrigger("Game.UI.InGame.TransportationOverviewUISystem","setLineName")&&CapturePolicy.IsGameplayTrigger("Game.UI.InGame.MapTilesUISystem","purchaseMapTiles")&&CapturePolicy.IsGameplayTrigger("Game.UI.InGame.UpgradesSection","deleteUpgrade"),"线路管理、购地与设施升级接入通用管理采集");
   Check(!CapturePolicy.IsGameplayTrigger("SomeMod.Settings","setApiKey")&&!CapturePolicy.IsGameplayTrigger("Game.UI.InGame.PoliciesUISystem","selectPolicy")&&!CapturePolicy.IsGameplayTrigger("Game.UI.InGame.TaxationUISystem","setCategory")&&!CapturePolicy.IsGameplayTrigger("Game.UI.InGame.CameraUISystem","setMode"),"设置密钥、导航选择与相机不进入采集");
   var metricDay=new DayLog{Day="2027-01-12"};
   Aggregator.Sample(metricDay,"population","人口","人",100,"08:00");Aggregator.Sample(metricDay,"population","人口","人",130,"09:00");Aggregator.Sample(metricDay,"population","人口","人",120,"10:00");Aggregator.Sample(metricDay,"bad","无效","",double.NaN,"10:00");
   var observed=metricDay.Metrics.Single();Check(observed.First==100&&observed.Last==120&&observed.Min==100&&observed.Max==130&&observed.Samples==3&&observed.FirstTime=="08:00"&&observed.LastTime=="10:00","指标保留实际观测区间的起末、极值，忽略无效数值");
   var observationHandler=new Fake();using(var observationService=new NewsService(observationHandler,Path.Combine(temp,"observation"))) {
    await observationService.Generate("metrics","汐澜",metricDay,prefs,"test",CancellationToken.None);
    Aggregator.Sample(metricDay,"population","人口","人",120,"11:00");
    var sameFacts=await observationService.Generate("metrics","汐澜",metricDay,prefs,"",CancellationToken.None);
    Check(sameFacts.LocalCache&&observationHandler.Calls==1,"仅有重复快照、不变的指标命中缓存");
    Aggregator.Sample(metricDay,"population","人口","人",140,"12:00");
    await observationService.Generate("metrics","汐澜",metricDay,prefs,"test",CancellationToken.None);Check(observationHandler.Calls==2,"指标实际变化使缓存失效，无操作也能按变化编报");
    var largeDay=new DayLog{Day="2027-01-13"};for(int i=0;i<601;i++)Aggregator.Add(largeDay,"城市实况","设施"+i,"10:00");
    await observationService.Generate("all","汐澜",largeDay,prefs,"test",CancellationToken.None);
    var allInput=Newtonsoft.Json.Linq.JObject.Parse((string)Newtonsoft.Json.Linq.JObject.Parse(observationHandler.Last)["messages"][1]["content"]);
    Check(allInput["actions"].Count()==1&&(int)allInput["actions"][0]["Count"]==601&&largeDay.Actions.Count==601,"同类建筑素材归并发送，完整原始记录留在本地");
   }
   Check(!GameCalendar.IsReady(DateTime.MinValue)&&!GameCalendar.IsValidDay("0001-01-01")&&!GameCalendar.IsValidDay("2027-99-12")&&GameCalendar.IsReady(new DateTime(2027,1,12,10,30,0)),"加载期默认日期无效，真实游戏时间可用");
   var legacyDays=new SaveState();legacyDays.Days.Add(new DayLog{Day="0001-01-01"});legacyDays.Days.Add(new DayLog{Day="2027-01-12"});
   Check(GameCalendar.RemoveInvalidDays(legacyDays)==1&&legacyDays.Days.Single().Day=="2027-01-12","旧存档移除虚假日期并保留正常历史");
   var emptyHistory=new SaveState();
   emptyHistory.Days.Add(new DayLog{Day="2021-01-06"});
   var earlyReal=new DayLog{Day="2021-01-07"};Aggregator.Add(earlyReal,"建设","学校","10:00");emptyHistory.Days.Add(earlyReal);
   var metricOnly=new DayLog{Day="2027-01-11"};Aggregator.Sample(metricOnly,"population","人口","人",100,"10:00");emptyHistory.Days.Add(metricOnly);
   emptyHistory.Days.Add(new DayLog{Day="2027-01-12"});
   Check(GameCalendar.PruneEmptyHistory(emptyHistory,"2027-01-12")==1&&emptyHistory.Days.All(d=>d.Day!="2021-01-06"),"清除加载期间遗留的过去空记录，不硬编码禁止 2021 年");
   Check(emptyHistory.Days.Count==3&&emptyHistory.Days.Contains(earlyReal)&&emptyHistory.Days.Contains(metricOnly),"保留早期真实建设、仅指标历史和当前尚未采集的日期");
   Check(GameCalendar.PruneEmptyHistory(emptyHistory,"")==0,"城市时间尚未就绪时不清理历史");
   var reloadedHistory=JsonConvert.DeserializeObject<SaveState>(JsonConvert.SerializeObject(emptyHistory));
   Check(!GameCalendar.IsVisibleIssue(reloadedHistory,"2021-01-06","2027-01-12")&&GameCalendar.IsVisibleIssue(reloadedHistory,"2021-01-07","2027-01-12"),"空日期已误生成的报纸隐藏且标记随存档保留，不删除本地归档");
   Check(GameCalendar.IsVisibleIssue(reloadedHistory,"2021-01-06","2021-01-06"),"实际读取到同日期的早期城市时允许正常阅读");
   bool invalidDayRejected=false;try{await service.Generate("invalid-day","汐澜",new DayLog{Day="0001-01-01"},prefs,"test",CancellationToken.None);}catch(InvalidOperationException){invalidDayRejected=true;}
   Check(invalidDayRejected&&handler.Calls==0,"无效日期不会发起 API 请求或生成报纸");
   var uneven=new[]{new Article{Title="A",Body=new string('a',500)},new Article{Title="B",Body=new string('b',100)},new Article{Title="C",Body=new string('c',100)},new Article{Title="D",Body=new string('d',100)}};
   var columns=PaperLayout.Group(uneven);
   Check(columns.Count==3&&columns.SelectMany(x=>x).SequenceEqual(uneven)&&columns.All(x=>x.Count>0),"报纸分栏保留文章顺序且不丢失内容");
   Check(PaperLayout.Group(uneven.Take(2).ToArray()).Count==2&&PaperLayout.Group(Array.Empty<Article>()).Count==1,"少量文章与空刊使用相同自适应分栏规则");
   var oldPreferences=new Preferences{Editorial=EditorialPolicy.Legacy};
   Check(EditorialPolicy.Upgrade(oldPreferences)&&oldPreferences.Editorial==EditorialPolicy.Default,"旧默认编辑方针自动升级");
   var previousPreferences=new Preferences{Editorial=EditorialPolicy.Previous};
   Check(EditorialPolicy.Upgrade(previousPreferences)&&previousPreferences.Editorial==EditorialPolicy.Default,"新闻价值筛选方针升级为专业日报措辞");
   var customPreferences=new Preferences{Editorial="我的自定义方针"};
   Check(!EditorialPolicy.Upgrade(customPreferences)&&customPreferences.Editorial=="我的自定义方针","保留玩家自定义编辑方针");
   var paper=await service.Generate("a","汐澜",log,prefs,"test",CancellationToken.None);
   Check(paper.Name=="汐澜日报"&&paper.CacheHitTokens==128,"自动报头与服务端缓存用量");
   paper=await service.Generate("a","汐澜",log,prefs,"",CancellationToken.None);
   Check(paper.LocalCache&&handler.Calls==1,"相同事实命中本地缓存且无需密钥");
   prefs.CustomName="市民时报";await service.Generate("a","汐澜",log,prefs,"test",CancellationToken.None);Check(handler.Calls==2,"改报头使缓存失效");
   prefs.Editorial+=" 精简篇幅";await service.Generate("a","汐澜",log,prefs,"test",CancellationToken.None);Check(handler.Calls==3,"编辑方针变化使缓存失效");
   await service.Generate("b","汐澜",log,prefs,"test",CancellationToken.None);Check(handler.Calls==4,"存档缓存隔离");
   handler.Invalid=true;bool rejected=false;try{await service.Generate("bad","汐澜",log,prefs,"test",CancellationToken.None);}catch(InvalidOperationException){rejected=true;}
   Check(rejected&&!Directory.Exists(Path.Combine(temp,"cities","bad","cache")),"无效结果不缓存");
   var root=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../"));
   if(!coreOnly)using(var binary=File.OpenRead(Path.Combine(root,"bin/Release/net48/CityPress.dll")))
   using(var pe=new PEReader(binary)) {
    var metadata=pe.GetMetadataReader();
    var calls=metadata.MemberReferences.Select(h=>metadata.GetString(metadata.GetMemberReference(h).Name)).ToArray();
    Check(!calls.Contains("GetRenderedLabelName")&&calls.Contains("get_cityName"),"城市名读取使用配置接口，不触发空本地化键");
   }
   Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory,"Templates"));File.Copy(Path.Combine(root,"Templates/newspaper.docx"),Path.Combine(AppContext.BaseDirectory,"Templates/newspaper.docx"),true);
   DocxExporter.TemplatePath=Path.Combine(root,"Templates/newspaper.docx");
   var output=Path.Combine(root,"artifacts/sample/汐澜日报.docx");DocxExporter.Save(output,paper);
   using(var zip=ZipFile.OpenRead(output)){using var reader=new StreamReader(zip.GetEntry("word/document.xml").Open());var xml=reader.ReadToEnd();Check(xml.Contains("城区建设持续推进")&&!xml.Contains("{{"),"Word 使用模板排版并替换示例正文");}
   // Reproduce the game's actual Assembly.Load(byte[]) behavior, not the
   // normal on-disk test assembly that hid the empty-location regression.
   if(!coreOnly) {
   var loaded=System.Reflection.Assembly.Load(File.ReadAllBytes(Path.Combine(root,"bin/Release/net48/CityPress.dll")));
   Check(string.IsNullOrEmpty(loaded.Location),"复现游戏内存加载时程序集路径为空");
   var exporter=loaded.GetType("CityPress.Core.DocxExporter");
   var savedPaper=JsonConvert.DeserializeObject(JsonConvert.SerializeObject(paper),loaded.GetType("CityPress.Core.Newspaper"));
   exporter.GetProperty("TemplatePath").SetValue(null,Path.Combine(temp,"missing-template.docx"));
   var recovered=Path.Combine(temp,"issues/byte-loaded.docx");
   exporter.GetMethod("Save").Invoke(null,new object[]{recovered,savedPaper});
   exporter.GetMethod("Save").Invoke(null,new object[]{recovered,savedPaper});
   using(var zip=ZipFile.OpenRead(recovered)){using var reader=new StreamReader(zip.GetEntry("word/document.xml").Open());Check(reader.ReadToEnd().Contains("城区建设持续推进"),"内存加载且外部模板缺失时内嵌模板可导出并覆盖日刊");}
   using(var zip=ZipFile.OpenRead(recovered)){
    using var reader=new StreamReader(zip.GetEntry("word/document.xml").Open());var doc=XDocument.Parse(reader.ReadToEnd());XNamespace w="http://schemas.openxmlformats.org/wordprocessingml/2006/main";
    var footer=doc.Descendants(w+"p").First(p=>(string)p.Element(w+"pPr")?.Element(w+"pStyle")?.Attribute(w+"val")=="CPFooter");
    Check(string.Concat(footer.Descendants(w+"t").Select(t=>t.Value)).Contains(paper.Name)&&!footer.ToString().Contains("{{"),"版脚与预览一致并替换实际报纸名称");
    Check(doc.Descendants(w+"tblCaption").Any(c=>(string)c.Attribute(w+"val")=="CityPress.Masthead")&&doc.Descendants(w+"t").Any(t=>t.Value=="电子刊")&&(string)doc.Root.Element(w+"background")?.Attribute(w+"color")=="F5F2E9","Word 保留左右版签、可编辑报头与纸色背景");
   }
   } else {
    DocxExporter.TemplatePath=Path.Combine(temp,"missing-template.docx");
    var fallback=Path.Combine(temp,"embedded-only.docx");DocxExporter.Save(fallback,paper);
    using(var zip=ZipFile.OpenRead(fallback)){using var reader=new StreamReader(zip.GetEntry("word/document.xml").Open());Check(reader.ReadToEnd().Contains("城区建设持续推进"),"无需游戏程序集的核心检查可验证内嵌模板回退");}
    Console.WriteLine("SKIP game-assembly metadata and byte-loading integration (--core-only)");
   }
   Console.WriteLine("All tests passed");
  }finally{if(Directory.Exists(temp))Directory.Delete(temp,true);}
 }
}
