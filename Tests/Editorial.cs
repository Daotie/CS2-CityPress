using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using CityPress.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

static class EditorialTests {
 static void Check(bool value,string message){if(!value)throw new Exception(message);Console.WriteLine("PASS "+message);}
 sealed class Scripted:HttpMessageHandler {
  public Func<string,string> Respond;public int Calls;public readonly List<string> Bodies=new List<string>();
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){
   Calls++;var body=await request.Content.ReadAsStringAsync();Bodies.Add(body);
   var payload=JObject.Parse(body);var system=(string)payload["messages"][0]["content"];
   var content=Respond(system);
   return new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonConvert.SerializeObject(new{choices=new[]{new{finish_reason="stop",message=new{content}}},usage=new{prompt_tokens=900,completion_tokens=600,prompt_tokens_details=new{cached_tokens=100}}}))};
  }
 }
 static Article Story(string section,string title,string body,string genre="消息")=>new Article{Section=section,Title=title,Body=body,Genre=genre};
 static DayLog MajorDay() {
  var day=new DayLog{Day="2027-01-12"};
  Aggregator.Sample(day,"city/population","常住人口","人",400381,"08:00");
  Aggregator.Sample(day,"city/population","常住人口","人",409931,"12:00");
  Aggregator.Sample(day,"city/money","市库现有资金","原生货币单位，资金存量",12965174,"08:00");
  Aggregator.Sample(day,"city/money","市库现有资金","原生货币单位，资金存量",18542974,"12:00");
  Aggregator.Sample(day,"city/households","家庭数量","户",195162,"08:00");
  Aggregator.Sample(day,"city/households","家庭数量","户",199064,"12:00");
  Aggregator.Sample(day,"city/monthlyIncome","月度预算总收入","原生货币单位/月",105593652,"08:00");
  Aggregator.Sample(day,"city/monthlyIncome","月度预算总收入","原生货币单位/月",105318514,"12:00");
  Aggregator.Sample(day,"city/monthlyExpenses","月度预算支出","原生货币单位/月",67363886,"08:00");
  Aggregator.Sample(day,"city/monthlyExpenses","月度预算支出","原生货币单位/月",68490786,"12:00");
  day.Breakthroughs.Add(new BreakthroughRecord{Key="city/population",Label="常住人口",Unit="人",Kind="数量级突破",Time="10:00",PreviousHigh=400000,Threshold=400000,Value=409931});
  day.Breakthroughs.Add(new BreakthroughRecord{Key="city/money",Label="市库资金余额",Unit="原生货币单位，资金存量",Kind="数量级突破",Time="11:00",PreviousHigh=12965174,Threshold=18000000,Value=18542974});
  Aggregator.Add(day,"城市实况 · 建筑进入施工","社区医疗中心","10:00");
  Aggregator.Add(day,"城市实况 · 建筑进入施工","现代中学","11:00");
  return day;
 }
 static string NewsBody()=>"全市常住人口升至约41万，市库资金余额突破1800万。常住人口由约40.04万增至约40.99万，家庭数量由约19.52万户增至19.91万户，市库资金余额由约1297万升至约1854万。医疗、教育与交通服务的安排需要与人口变化同步。";
 static string CommentaryBody()=>"人口与资金的同步变化，是观察城市运行的一扇窗口，但不能直接等同于公共服务已经改善。\n常住人口与家庭数量上升意味着教育、医疗和交通需求会持续增加；市库资金余额上升，则说明城市在当期拥有更大的资金存量。两者都只是起点，关键还要看资金能否转化为稳定服务。\n从长期看，财政安排必须把建设与维护一并考虑。工程投入带来一次性的支出，而学校、医院和公交线路需要长期运营成本，如果只看资金余额增加，就容易忽视后续压力。\n因此，讨论城市发展时应区分已确认的观测事实与有条件的影响：人口增长能否带来更强的公共服务能力，取决于设施分布、人员配置和预算取舍。把短期资金变化当作长期繁荣的证明，是一种需要避免的推断。\n对市民而言，真正重要的不是数字本身，而是这些数字背后能否形成可持续的服务安排。城市管理需要在这一目标下权衡投入节奏与公共利益的优先级。";
 static Newspaper FirstPaper()=>new Newspaper{Name="汐澜日报",Day="2027-01-12",Headline="市库资金余额与常住人口同步走高",Deck="城市财政与人口指标出现新的变化。",Provider="test",Model="model",Articles=new List<Article>{
  Story("城市要闻","常住人口与市库资金余额同步走高",NewsBody()),
  Story("城市建设与交通","社区医疗中心与学校进入建设阶段","社区医疗中心的建设正在推进，周边道路与步行通道同步调整，学校与医院等公共服务设施的布局需要与人口分布匹配。"),
  Story("民生与社会","家庭数量随人口增长上升","家庭数量由约19.52万户增至19.91万户，公共服务需求随之增加，设施分布与人员配置需要进一步统筹。"),
  Story("环境与观察","空气与土壤污染强度出现波动","全地图分批观测显示污染强度有所波动，具体影响仍需结合后续观测判断，供水与污水处理能力保持稳定。")
 }};
 static string CommentaryJson()=>JsonConvert.SerializeObject(new{title="城市增长的质量比数字更重要",deck="人口与资金变化之后，公共服务仍需回答长期问题",section="评论",body=CommentaryBody()});
 static DayLog referenceDayForRevision(){
  var day=new DayLog{Day="2027-01-12"};
  Aggregator.Sample(day,"city/population","常住人口","人",400000,"08:00");Aggregator.Sample(day,"city/population","常住人口","人",410000,"12:00");
  Aggregator.Sample(day,"city/money","市库现有资金","货币",15000000,"08:00");Aggregator.Sample(day,"city/money","市库现有资金","货币",18000000,"12:00");
  Aggregator.Sample(day,"city/monthlyIncome","月度预算总收入","货币/月",100000000,"08:00");Aggregator.Sample(day,"city/monthlyIncome","月度预算总收入","货币/月",105000000,"12:00");
  return day;
 }
 public static async Task Run(string temp) {
  // 1. News value: important events are graded, but a bare threshold is not enough for a commentary.
  var rich=MajorDay();var richBrief=NewsValue.Evaluate(rich,"汐澜");
  Check(richBrief.HasMajor&&richBrief.MajorEvents.Any(item=>item.MajorToken=="常住人口")&&richBrief.CommentaryWarranted&&!string.IsNullOrWhiteSpace(richBrief.CommentaryFocus),"重大突破被评定为重大事件并满足独立评论的事实条件");
  var sparse=new DayLog{Day="2027-01-12"};sparse.Breakthroughs.Add(new BreakthroughRecord{Key="city/money",Label="市库资金余额",Unit="货币",Kind="数量级突破",Time="10:00",PreviousHigh=12000000,Threshold=15000000,Value=16000000});
  Check(!NewsValue.Evaluate(sparse,"汐澜").CommentaryWarranted,"仅有单个阈值突破、缺少其他事实时不强行生成评论");
  var monthly=new DayLog{Day="2027-01-12"};monthly.Breakthroughs.Add(new BreakthroughRecord{Key="city/monthlyBalance",Label="月度预算收支结余",Unit="货币",Kind="数量级突破",Time="10:00",PreviousHigh=1,Threshold=2,Value=3});
  Check(!NewsValue.Evaluate(monthly,"汐澜").HasMajor,"旧错误财政口径不参与重大事件与评论判定");

  // 2. Deterministic page plan turns a flat mixed list into themed editions.
  var mixed=FirstPaper();int expectedArticles=mixed.Articles.Count;mixed.Headline="多主题版面校样";mixed.Articles.AddRange(new List<Article>{
   Story("经济与财政","月度预算结余保持稳定","市库资金余额与月度预算收入、支出结构保持稳定，财政安排需要兼顾建设与维护。"),
   Story("环境与观察","空气与土壤污染监测出现波动","空气与土壤污染监测强度出现波动，环境指标需要结合后续观测判断。"),
   Story("民生与社会","城区发生一起建筑火灾","城区发生一起建筑火灾，未造成人员伤亡，相关处置已经结束，具体原因未见进一步信息。"),
   Story("城市评论","城市增长的质量比数字更重要",CommentaryBody(),"评论")
  });expectedArticles=mixed.Articles.Count;
  var mixedBrief=NewsValue.Evaluate(rich,"汐澜");var plan=PagePlanner.Plan(mixed,mixedBrief);
  Check(plan.Pages.Count>=4&&plan.Pages.All(page=>page.Articles.Count>0),"多领域素材被规划为多个非空主题版，不再全部塞进综合版");
  Check(plan.Pages.All(page=>!PagePlanner.IsGenericTheme(page.Theme)),"规划后的每个新闻版都有明确主题");
  Check(plan.Pages.Last().Theme=="评论"&&plan.Pages.Last().Articles.All(PagePlanner.IsCommentary),"评论独立成版并排在最后");
  Check(plan.Pages.SelectMany(page=>page.Articles).Count()==expectedArticles,"分版不丢失也不重复任何稿件");
  Check(mixed.Pages.All(page=>page.Composition==null),"版面规划阶段尚未保存版式，留给排版阶段统一生成");
  var tiny=new Newspaper{Name="汐澜日报",Day="2027-01-12",Headline="城市短讯",Deck="",Articles=new List<Article>{
   Story("城市建设与交通","一段道路完成施工","城区一段道路完成施工并开放通行，周边步行通道同步调整。"),
   Story("民生与社会","社区服务点启用","社区服务点开始使用，为周边居民提供日常服务。")
  }};
  var tinyPlan=PagePlanner.Plan(tiny,new EditorialBrief());
  Check(tinyPlan.Pages.Count==1&&tinyPlan.Pages[0].Articles.Count==2,"素材很少时不强凑多版或空白版面");

  // 3. Empty editions are dropped rather than treated as a fatal model error.
  var emptyPage=new Newspaper{Headline="空版兼容",Pages=new List<NewspaperPage>{new NewspaperPage{Theme="要闻",Headline="新闻",Articles=new List<Article>{Story("要闻","正常新闻","这是一条正常新闻的正文内容，用于验证空版处理。")}},new NewspaperPage{Theme="空版",Articles=new List<Article>()}}};
  EditionRules.Normalize(emptyPage);Check(emptyPage.Pages.Count==1,"模型返回空版面时丢弃空版而不是拒绝整份报纸");

  // 4. End-to-end: news + dedicated commentary + persistence + Word export.
  var paper=FirstPaper();
  var scripted=new Scripted{Respond=system=>{
   if(system.Contains("评论编辑"))return CommentaryJson();
   return JsonConvert.SerializeObject(paper);
  }};
  var prefs=new Preferences();
  var root=Path.Combine(temp,"pipeline");
  using(var service=new NewsService(scripted,root)){
   var before=MajorDay();var result=await service.Generate("save","汐澜",before,prefs,"key",CancellationToken.None).ConfigureAwait(false);
   Check(scripted.Calls==2&&result.Pages.Count>=4,"重大事件先出新闻，再通过独立评论任务补写评论（两次请求）");
   Check(result.Pages.Last().Theme=="评论"&&result.Pages.Last().Articles.All(article=>article.Genre=="评论"&&article.Title=="城市增长的质量比数字更重要"),"评论是独立文章对象并占据独立评论版");
   Check(result.Articles.Count(article=>article.Genre=="评论")==1&&result.Articles.Count(article=>article.Genre!="评论")>=3,"评论未被重复并入普通新闻，其他主题稿保留");
   Check(result.QualityScore>=70&&result.Articles.All(article=>!article.Issues.Any(issue=>issue.Severity=="fatal")),"生成结果通过事实与质量检查");
   Check(result.Articles.Any(article=>article.Genre!="评论"&&article.Sources.Count>0&&!string.IsNullOrEmpty(article.Id)),"稿件保存事件来源、编号与质量审计元数据");
   Check(!string.IsNullOrWhiteSpace(result.CommentaryFocus)&&result.LayoutVersion=="7","评论触发依据与版式版本随报纸保存");
   var reloaded=JsonConvert.DeserializeObject<Newspaper>(JsonConvert.SerializeObject(result));
   Check(reloaded.Pages.Count==result.Pages.Count&&PagePlanner.HasCommentary(reloaded),"重新加载后版面与独立评论不丢失");
   Check(reloaded.Pages.All(page=>page.Composition!=null),"版式计划随报纸一起持久化");
   var store=new ExportStore(Path.Combine(temp,"archive-data"));
   var saved=store.Save("save",reloaded,Path.Combine(temp,"export-root"),CancellationToken.None);
   var read=store.Read("save",reloaded.Day,Path.Combine(temp,"export-root"));
   Check(read.Paper.Pages.Count==result.Pages.Count&&PagePlanner.HasCommentary(read.Paper),"归档 JSON 保存多版内容与评论");
   var docx=Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../")),"artifacts/sample/多版评论校样.docx");
   DocxExporter.Save(docx,read.Paper);
   using(var zip=ZipFile.OpenRead(docx))using(var stream=zip.GetEntry("word/document.xml").Open()){
    var doc=XDocument.Load(stream);XNamespace w="http://schemas.openxmlformats.org/wordprocessingml/2006/main";
    var text=string.Concat(doc.Descendants(w+"t").Select(node=>node.Value));
    int breaks=doc.Descendants(w+"br").Count(node=>(string)node.Attribute(w+"type")=="page");
    Check(breaks==result.Pages.Count-1&&text.Contains("评论版")&&text.Contains("城市增长的质量比数字更重要")&&text.Contains("城市建设与交通版"),"Word 导出包含全部版面、显式分页与独立评论");
   }
   var calls=scripted.Calls;var cached=await service.Generate("save","汐澜",MajorDay(),prefs,"",CancellationToken.None).ConfigureAwait(false);
   Check(cached.LocalCache&&scripted.Calls==calls&&cached.Pages.Count==result.Pages.Count,"相同事实命中本地缓存且不重新分版");
  }

  // 5. A failed commentary is reported as a failure, never published as news-only.
  var failureHandler=new Scripted{Respond=system=>system.Contains("评论编辑")?JsonConvert.SerializeObject(FirstPaper()):JsonConvert.SerializeObject(FirstPaper())};
  using(var service=new NewsService(failureHandler,Path.Combine(temp,"failed"))) {
   bool failed=false;string message="";
   try{await service.Generate("fail","汐澜",MajorDay(),new Preferences(),"key",CancellationToken.None).ConfigureAwait(false);}
   catch(InvalidOperationException error){failed=true;message=error.Message;}
   Check(failed&&message.Contains("评论或分析")&&failureHandler.Calls>=2,"独立评论生成失败时明确报错并重试，不假装完成");
   Check(!Directory.Exists(Path.Combine(temp,"failed","cities","fail","cache")),"评论失败时不写入缓存");
  }

  // 6. One targeted revision repairs flagged copy without rewriting the paper.
  var revisionHandler=new Scripted{Respond=system=>system.Contains("质量编辑")
   ? "{\"rewrites\":[{\"index\":0,\"title\":\"人口达到新规模\",\"body\":\"全市常住人口达到约41万人，家庭数量与公共服务需求同步变化，设施布局需要与人口分布匹配。\"}]}"
   : JsonConvert.SerializeObject(new Newspaper{Headline="事实核验",Articles=new List<Article>{Story("民生","人口达到新规模","全市常住人口已经达到45万人，城市发展进入新阶段，公共服务需要同步跟进，设施布局也应随人口变化调整。")}})};
  using(var service=new NewsService(revisionHandler,Path.Combine(temp,"revision"))) {
   var result=await service.Generate("rev","汐澜",referenceDayForRevision(),new Preferences(),"key",CancellationToken.None).ConfigureAwait(false);
   Check(revisionHandler.Calls==2&&result.RevisionCount==1&&result.Articles[0].Body.Contains("41万")&&result.Articles.All(article=>!article.Issues.Any(issue=>issue.Severity=="fatal")),"事实错误通过一次局部修订修正，不整份重写");
  }

  // 7. Fact, language, duplication and commentary checks.
  var facts=new DayLog{Day="2027-01-12"};
  Aggregator.Sample(facts,"city/population","常住人口","人",400000,"08:00");Aggregator.Sample(facts,"city/population","常住人口","人",410000,"12:00");
  Aggregator.Sample(facts,"city/money","市库现有资金","货币",15000000,"08:00");Aggregator.Sample(facts,"city/money","市库现有资金","货币",18000000,"12:00");
  Aggregator.Sample(facts,"city/monthlyIncome","月度预算总收入","货币/月",100000000,"08:00");Aggregator.Sample(facts,"city/monthlyIncome","月度预算总收入","货币/月",105000000,"12:00");
  Aggregator.Sample(facts,"city/monthlyExpenses","月度预算支出","货币/月",60000000,"08:00");Aggregator.Sample(facts,"city/monthlyExpenses","月度预算支出","货币/月",68000000,"12:00");
  Aggregator.Sample(facts,"native/Unemployed/0","Unemployed","人",12221,"08:00");Aggregator.Sample(facts,"native/Unemployed/0","Unemployed","人",15263,"12:00");
  var fabricated=new Newspaper{Headline="事实核验",Articles=new List<Article>{Story("民生","人口达到新规模","全市常住人口已经达到45万人，城市发展进入新阶段。")}};
  Check(QualityControl.Review(fabricated,new EditorialBrief(),facts).HasFatal,"无法追溯到采集数据的数字被判定为严重事实问题");
  var traceable=new Newspaper{Headline="事实核验",Articles=new List<Article>{Story("民生","人口达到新规模","全市常住人口达到约41万人，失业人数由约1.22万人升至约1.53万人，家庭数量与公共服务需求同步变化。")}};
  Check(!QualityControl.Review(traceable,new EditorialBrief(),facts).HasFatal,"可追溯到原始数据的数字可以通过核验");
  var invented=new Newspaper{Headline="来源核验",Articles=new List<Article>{Story("建设","社区医疗中心开工","据有关部门介绍，社区医疗中心即将投入使用并带来显著成效。")}};
  var inventedReview=QualityControl.Review(invented,new EditorialBrief(),facts);
  Check(inventedReview.HasFatal&&inventedReview.Issues.Any(issue=>issue.Code=="fabricated-source"),"虚构采访、引述或消息来源被拦截");
  var mechanical=new Newspaper{Headline="语言核验",Articles=new List<Article>{Story("建设","项目建设成效显著","项目建设成效显著，城市面貌持续改善，为未来发展奠定基础。")}};
  Check(QualityControl.Review(mechanical,new EditorialBrief(),facts).Issues.Any(issue=>issue.Code=="mechanical-evaluation"),"缺少事实支撑的评价性套话被记录并触发修订");
  var duplicate=new Newspaper{Headline="重复核验",Articles=new List<Article>{Story("建设","社区医疗中心进入施工","社区医疗中心进入施工阶段，周边道路与步行通道同步调整，服务设施布局需要与人口分布匹配。"),Story("建设","社区医疗中心进入施工","社区医疗中心进入施工阶段，周边道路与步行通道同步调整，服务设施布局需要与人口分布匹配。")}};
  var duplicateReview=QualityControl.Review(duplicate,new EditorialBrief(),facts);
  Check(duplicateReview.Issues.Any(issue=>issue.Code=="duplicate-news"),"同一事件重复成篇被识别");
  Check(QualityControl.ApplySafeDegradation(duplicate,duplicateReview)&&duplicate.Articles.Count==1&&duplicate.DroppedUnsafeArticles==1,"安全降级删除重复稿件而不是发布两份相同报道");
  var copyNews=Story("建设","社区医疗中心进入施工","社区医疗中心进入施工阶段，周边道路与步行通道同步调整，服务设施布局需要与人口分布匹配，后续运营安排仍需核实。");
  var copy=new Newspaper{Headline="评论重复核验",Articles=new List<Article>{copyNews,Story("评论","评论只是重抄新闻",copyNews.Body,"评论")}};
  var copyReview=QualityControl.Review(copy,new EditorialBrief(),facts);
  Check(copyReview.CommentaryCopy&&copyReview.HasFatal,"评论重抄新闻正文被判为严重问题，不会被当作独立评论发布");
  var shortComment=new Newspaper{Headline="评论长度核验",Articles=new List<Article>{Story("城市","城市新闻","城市公共服务的安排需要考虑设施分布与居民需求，这是一个持续的过程。"),Story("评论","过短的评论","公共投入需要更加重视服务效果。","评论")}};
  Check(QualityControl.Review(shortComment,new EditorialBrief(),facts).Issues.Any(issue=>issue.Code=="commentary-too-short"),"过短的评论不能冒充深度评论");
  foreach(var article in new[]{fabricated.Articles[0],traceable.Articles[0],mechanical.Articles[0]})article?.Issues.Clear();
  Console.WriteLine("Editorial pipeline checks passed");
 }
}
