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
 static string Compact(string text)=>string.Concat((text??"").Where(character=>!char.IsWhiteSpace(character)));
 static string LongCommentaryBody()=>string.Join("\n",new[]{
  "市库资金余额突破1800万，是这座城市财政运行中的一个节点。但余额上升只说明账面可用资金增加，并不自动等于公共服务水平提高，判断财政健康还需要结合收入、支出与长期服务需求。",
  "## 资金存量不是服务成效",
  "资金余额与月度预算收入、支出属于不同口径。余额增加可能来自收入节奏、结算时点或支出推迟，单看一个数字无法判断财政状况是否真正改善。把余额增长当成可以长期扩大支出的依据，是一种需要警惕的推断。",
  "## 服务需求的持续时间",
  "常住人口增至约41万，意味着学校、医疗和交通需求会持续存在。这些服务一旦投入使用，就会产生长期运营成本，城市在做投入决定时应把建设费用与后续维护、人员开支放在同一张账上考虑。",
  "## 更长期的选择",
  "衡量财政健康不能只看余额高低，还要看收入是否稳定、支出是否与真实需求匹配、公共服务是否形成可持续供给。资金充裕为城市提供选择空间，但选择本身仍需公共利益的衡量，并需要关注长期维护带来的财政压力。",
  "对市民而言，真正重要的是这些资金能否转化为可感知、可负担、可长期维持的服务。城市管理的选择因此更应谨慎，把短期资金变化与长期服务能力区分开来，在预算安排中保留应对不确定支出的空间，并对服务质量的后续变化保持关注。",
  "财政数字本身不构成结论，关键在于公共投入能否对应真实需求。收入结构、支出优先级和运营维护安排共同决定服务效果，任何一项短板都可能削弱资金的实际价值，这需要更长周期的观察与评估。"
 });
 static Article LongCommentaryArticle(string title="市库突破之后，城市更要回答钱怎么花"){var article=Story("评论",title,LongCommentaryBody(),"评论");article.CommentaryRole="长评";return article;}
 static Article ShortCommentArticle(string title,string body){var article=Story("短评",title,body,"评论");article.CommentaryRole="短评";return article;}
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

  // 8. Long commentary carries subheads; short comments share its edition.
  Check(PaperLayout.BodyLength(LongCommentaryBody())>=500&&PaperLayout.Paragraphs(LongCommentaryBody()).Count(PaperLayout.IsSubhead)==3,"测试用长评论达到小标题门槛并含三个小标题");
  var shortOne=ShortCommentArticle("资金要用在服务上","资金余额上升说明城市拥有更多选择，但真正影响市民的是这些钱能否变成可负担、可持续的公共服务。关键在于预算安排是否对应真实需求，而不是只看余额数字。长期维护成本同样需要提前考虑，否则短期充裕可能变成未来的财政压力，公共服务的质量也就难以稳定。城市在做预算选择时，应把服务覆盖范围、人员配置与后续维护放在同一张账上权衡，避免把一次性投入当成长期能力。");
  var shortTwo=ShortCommentArticle("人口增长提出新要求","人口增至约41万意味着学校、医院和交通的压力会持续存在，设施布局与人员配置需要同步跟上。长期看，服务能力取决于持续投入与维护，而不是一次性的工程数量。公共服务的衔接需要更细致的安排：新建设施能否真正开放、服务半径是否覆盖新增人口、运营成本是否可承受，都会影响市民的实际体验，也决定城市扩张的质量。这些判断需要结合后续可核实的数据，不能只靠工程数量下结论。");
  var leadCommentary=LongCommentaryArticle();
  var commentaryPage=new NewspaperPage{Theme="评论",Headline=leadCommentary.Title,Deck="",Articles=new List<Article>{leadCommentary,shortOne,shortTwo}};
  var mixedCommentary=new Newspaper{Name="汐澜日报",Day="2027-01-12",Headline="评论与短评校样",Deck="",Pages=new List<NewspaperPage>{new NewspaperPage{Theme="要闻",Headline="城市新闻",Articles=new List<Article>{Story("要闻","人口与资金变化","常住人口与市库资金余额出现新的变化，公共服务需要同步安排。")}},commentaryPage}};
  mixedCommentary.Articles=mixedCommentary.Pages.SelectMany(page=>page.Articles).ToList();
  PaperLayout.Assign(mixedCommentary,"auto");
  Check(commentaryPage.Composition.Template=="opinion-mixed"&&commentaryPage.Composition.Columns.Count==3,"长评论与短评共用一版时使用主版加短评侧栏版式");
  Check(commentaryPage.Composition.Columns.Last().Width<commentaryPage.Composition.Columns[0].Width,"短评位于窄侧栏，长评论占据大部分版面");
  var mainBlocks=commentaryPage.Composition.Columns.Take(2).SelectMany(column=>column.Blocks).ToList();
  Check(mainBlocks.Count(block=>block.Kind=="subhead")==3,"长评论小标题作为独立内容块进入排版");
  var mainText=string.Concat(mainBlocks.Where(block=>block.Kind=="body"||block.Kind=="subhead").Select(block=>block.Kind=="subhead"?"## "+PaperLayout.BlockText(commentaryPage,block):PaperLayout.BlockText(commentaryPage,block)));
  Check(Compact(mainText).Contains(Compact("资金存量不是服务成效"))&&Compact(mainText).Contains(Compact("服务需求的持续时间"))&&Compact(mainText).Contains(Compact("更长期的选择")),"小标题与正文完整进入主版");
  Check(commentaryPage.Composition.Columns.Last().Blocks.Any(block=>block.Kind=="title"&&PaperLayout.BlockText(commentaryPage,block)==shortOne.Title),"短评标题位于侧栏");
  var longReview=QualityControl.Review(new Newspaper{Headline="评论质量",Articles=new List<Article>{LongCommentaryArticle(),shortOne,shortTwo}},new EditorialBrief(),referenceDayForRevision());
  Check(!longReview.Issues.Any(issue=>issue.Code=="commentary-no-subheads")&&!longReview.HasFatal,"带小标题的长评论与短评通过评论专项质量检查");
  var plainLong=Story("评论","缺少层次的长评论",LongCommentaryBody().Replace("## ","").Replace("\n",""),"评论");
  var plainReview=QualityControl.Review(new Newspaper{Headline="评论质量",Articles=new List<Article>{plainLong}},new EditorialBrief(),referenceDayForRevision());
  Check(plainReview.Issues.Any(issue=>issue.Code=="commentary-no-subheads")&&plainReview.NeedsRevision,"长评论缺少小标题时触发修订要求");

  // 9. A rich single topic receives extra editions instead of being crammed.
  var repeated="市库资金余额与月度预算收支结构保持稳定，收入、支出与结余需要分别理解。产业、商业与就业数据的变化说明，财政安排应把建设投入与长期运营成本放在一起考虑，公共服务水平取决于持续投入与维护，而不是一次性的资金规模。";
  var longBody=string.Join("\n",Enumerable.Repeat(repeated,4));
  var economyArticles=new List<Article>();
  for(int i=1;i<=11;i++)economyArticles.Add(Story("经济与财政","预算与产业观察之"+i,longBody));
  var economyPaper=new Newspaper{Name="汐澜日报",Day="2027-01-12",Headline="经济与财政专版",Deck="",Articles=economyArticles};
  var economyPlan=PagePlanner.Plan(economyPaper,new EditorialBrief());
  Check(economyPlan.Pages.Count>=3&&economyPlan.Pages.All(page=>PagePlanner.BaseTheme(page.Theme)=="经济与财政")&&economyPlan.Pages.Skip(1).All(page=>page.Theme.StartsWith("经济与财政（续",StringComparison.Ordinal)),"同一主题素材多时连续加版并使用（续）版名");
  Check(economyPaper.Articles.Count==11&&economyPlan.Pages.All(page=>page.Articles.Count>0),"加版不丢失稿件也不产生空版");

  // 10. End-to-end: dedicated commentary returns a long piece with subheads and short comments.
  var mixedCommentaryJson=JsonConvert.SerializeObject(new{title=leadCommentary.Title,deck="资金存量之后，服务能力才是长期问题",section="评论",body=LongCommentaryBody(),shortComments=new[]{new{title=shortOne.Title,body=shortOne.Body},new{title=shortTwo.Title,body=shortTwo.Body}}});
  var mixedHandler=new Scripted{Respond=system=>system.Contains("评论编辑")?mixedCommentaryJson:JsonConvert.SerializeObject(FirstPaper())};
  using(var service=new NewsService(mixedHandler,Path.Combine(temp,"mixed-commentary"))){
   var result=await service.Generate("mixed","汐澜",MajorDay(),new Preferences(),"key",CancellationToken.None).ConfigureAwait(false);
   var page=result.Pages.Last();
   Check(mixedHandler.Calls==2&&page.Theme=="评论"&&page.Articles.Count==3,"独立评论任务可同时返回长评论与短评并放入同一评论版");
   Check(page.Articles[0].CommentaryRole=="长评"&&page.Articles.Skip(1).All(article=>article.CommentaryRole=="短评"),"长评论与短评角色区分，长评论排在最前");
   Check(page.Composition!=null&&page.Composition.Template=="opinion-mixed","评论版按长评论加短评版式排版");
   Check(result.QualityScore>=70&&result.Articles.All(article=>!article.Issues.Any(issue=>issue.Severity=="fatal")),"长评论与短评通过质量检查");
   var output=Path.Combine(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../")),"artifacts/sample/评论与短评校样.docx");
   DocxExporter.Save(output,result);
   using(var zip=ZipFile.OpenRead(output))using(var stream=zip.GetEntry("word/document.xml").Open()){
    var doc=XDocument.Load(stream);XNamespace w="http://schemas.openxmlformats.org/wordprocessingml/2006/main";
    var text=string.Concat(doc.Descendants(w+"t").Select(node=>node.Value));
    Check(text.Contains("资金存量不是服务成效")&&text.Contains("服务需求的持续时间")&&text.Contains(shortOne.Title)&&text.Contains(shortTwo.Title),"Word 导出保留小标题与短评标题");
    Check(text.Split(new[]{"资金存量不是服务成效"},StringSplitOptions.None).Length-1==1,"小标题在 Word 中只出现一次，未与正文重复");
   }
  }
  Console.WriteLine("Editorial pipeline checks passed");
 }
}
