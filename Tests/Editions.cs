using System;
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
static class EditionTests {
 static void Check(bool value,string message){if(!value)throw new Exception(message);Console.WriteLine("PASS "+message);}
 sealed class Response:HttpMessageHandler {
  public Newspaper Paper;public string Request;public int Calls;
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;Request=await request.Content.ReadAsStringAsync();return new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent(JsonConvert.SerializeObject(new{choices=new[]{new{finish_reason="stop",message=new{content=JsonConvert.SerializeObject(Paper)}}}}))};}
 }
 static Article Story(string title,string body,string genre="消息")=>new Article{Section=genre=="评论"?"城市短评":"城市",Genre=genre,Title=title,Body=body};
 public static async Task Run(string temp,string project){
  Check(FinanceFacts.MonthlyNet(105000000,68000000)==37000000&&FinanceFacts.MonthlyNet(100,140)==-40,"财政原生正支出从收入中扣除，避免把收入加支出误报结余");
  var waiting=new TransitRecord{Id="line3",Name="APM 3 号线",Mode="地铁",Enabled=true,Stops=8,Fare=6};var operating=TransitFacts.Copy(waiting);operating.Operating=true;operating.Vehicles=3;
  Check(!TransitFacts.IsOperating(true,8,0)&&!TransitFacts.IsOperating(false,8,3)&&!TransitFacts.IsOperating(true,1,3)&&TransitFacts.IsOperating(true,8,3),"线路启用和配车不独立证明运营，须有停站及实际服务车辆");
  Check(TransitFacts.Transition(null,operating,true)==""&&TransitFacts.Transition(waiting,operating,false)=="投入运营"&&TransitFacts.Transition(operating,operating,false)=="","运营初次基线不追认开通；由待发到实际服务才记录开通，持续服务不重复报");
  Check(TransitFacts.Transition(waiting,operating,false,true)=="","既有线路夜间发车或短暂车辆空档后服务恢复，不冒充新线开通");
  var day=new DayLog{Day="2027-01-12"};operating.Change="投入运营";TransitFacts.Store(day,operating);var repeated=TransitFacts.Copy(operating);repeated.Time="18:30";repeated.Change="";TransitFacts.Store(day,repeated);
  Check(day.Transit.Count==1&&day.Transit[0].Name=="APM 3 号线"&&day.Transit[0].Change=="投入运营","线路按对象合并，保留真实名称与当日开通事实");
  var priceChange=TransitFacts.Copy(repeated);priceChange.Change="票价调整";TransitFacts.Store(day,priceChange);Check(day.Transit[0].Change=="投入运营；票价调整","同日后续调价不会覆盖新线开通事实");
  var snapshot=JournalSnapshot.CopyDay(day);day.Transit[0].Name="更名";Check(snapshot.Transit[0].Name=="APM 3 号线","后台快照隔离可变线路事实");day=snapshot;
  var paper=new Newspaper{Name="示例市日报",Day=day.Day,Headline="轨道交通与公共服务",Deck="排版测试样张",Pages=new(){
   new NewspaperPage{Theme="要闻",Headline="APM 3 号线投入运营",Deck="地铁服务增添新线路",Articles=new(){Story("APM 3 号线投入运营","APM 3 号线已投入运营，设有8个停站，票价为6元。\n线路服务与市民出行密切相关。"),Story("人口跨过四十万","常住人口达到40.46万，新增人口带来的学校、医院、住房与交通需求，应纳入下一阶段的城市规划与公共服务安排。")}},
   new NewspaperPage{Theme="经济",Headline="月度预算保持结余",Deck="统筹财政资源与公共服务",Articles=new(){Story("月度预算结余3700万元","月度预算收入为1.05亿元，支出为6800万元，结余3700万元。\n市库余额为1500.8万元。"),Story("让预算安排回应服务需求","人口规模扩大，公共服务的投入方向值得重视。预算安排应结合学校、医疗与交通服务需求，统筹建设和后续支出。","分析")}},
   new NewspaperPage{Theme="评论",Headline="新线开通之后还要做好服务衔接",Deck="围绕城市交通的短评",Articles=new(){Story("把线路延伸变成出行便利","APM 3 号线投入运营，为讨论公共交通服务衔接提供了新的切入点。\n一条线路的价值需要通过换乘组织、站点可达性和服务稳定性体现。线路开通只说明服务能力发生变化，并不意味着市民的出行方式会立刻改变。\n从长期看，新线能否发挥作用取决于与其他交通方式的衔接：站点周边的步行环境、接驳线路的安排，以及高峰时段的运力配置都会影响实际体验。\n因此，后续管理宜把接驳安排与服务稳定性放在同等位置，在有实际客流依据后调整运力，并持续关注运营成本与公共服务水平之间的平衡。\n对城市而言，交通投入的衡量标准不是线路数量，而是服务是否覆盖真实需求。票价、班次和换乘时间共同决定出行成本，这些细节比开通本身更值得长期跟踪。只有在运营数据公开、服务调整有据可依时，新线才可能真正改变市民的通勤选择。","评论"),Story("财政安排需要长期眼光","本期月度预算保持结余，给城市公共服务的安排提供了讨论基础。\n预算结余说明当期收入高于支出，但它只是预算口径的结果，不能替代对资金存量的判断，也不能直接证明公共服务已经改善。\n从长期看，财政安排应把建设与维护一并考虑：人口规模扩大意味着学校、医疗和交通服务会持续产生支出，一次性的工程投入之后还有长期运营成本。\n对服务需求的判断还应结合人口与设施分布，让预算选择有明确的公共目标，并保留应对不确定支出的空间，避免把短期结余当成可以无限扩大的支出依据。\n预算数字本身并不说明服务质量，关键在于资金投向能否对应真实需求。财政透明度、项目优先级和后续维护安排共同决定公共投入的效果，这些都需要在更长周期内观察和评估。","评论")}}
  }};
  var lead=paper.Pages.Last().Articles[0];
  lead.Body=lead.Body.Replace("从长期看", "## 接驳决定服务价值\n从长期看").Replace("对城市而言", "## 让服务回应日常出行\n对城市而言")+"\n公共交通服务的长期选择，应当同时考虑日常通勤与非高峰出行的不同需求。站点附近的步行联系、跨线换乘的组织方式和服务时间之间存在相互影响，任何一处衔接不畅，都可能让新建线路难以形成完整的出行链条。后续调整宜以实际出行需求为依据，在运行中识别堵点，并把改善服务的投入与维护责任一并安排。这既关乎交通设施的使用，也关乎不同居住片区能否公平获得公共服务。";
  EditionRules.Normalize(paper);Check(paper.Articles.Count==6&&PaperLayout.Pages(paper).Count==3,"分版稿按版面保留顺序，同时生成兼容的完整文章列表");
  Check(PaperLayout.Pages(new Newspaper{Headline="旧刊",Articles=new(){Story("历史消息","正文")}}).Single().Theme=="综合","旧单版报纸无需迁移即可阅读与导出");
  var oldPrefs=new Preferences{Editorial=EditorialPolicy.PriorProfessional};Check(EditorialPolicy.Upgrade(oldPrefs)&&oldPrefs.Editorial==EditorialPolicy.Default,"beta.4默认编辑方针自动升级，保留自定义设置机制");
  using(var handler=new Response{Paper=paper})using(var service=new NewsService(handler,Path.Combine(temp,"editions"))){
   Aggregator.Sample(day,"city/monthlyBalance","旧错误结余","",173000000,"18:00");day.Breakthroughs.Add(new BreakthroughRecord{Key="city/monthlyBalance",Value=173000000});
   var result=await service.Generate("edition","示例市",day,new Preferences(),"fake",CancellationToken.None);var request=JObject.Parse(handler.Request);var facts=JObject.Parse((string)request["messages"][1]["content"]);
   Check(facts["transit"][0]["Name"].ToString()=="APM 3 号线"&&(bool)facts["transit"][0]["Operating"]&&facts["metrics"].Count()==0&&facts["breakthroughs"].Count()==0,"线路原名与实际服务进入素材，旧错误财政指标及突破隔离");
   repeated.Time="18:40";repeated.Name="APM 3 号线";TransitFacts.Store(day,repeated);await service.Generate("edition","示例市",day,new Preferences(),"",CancellationToken.None);Check(handler.Calls==1,"普通线路观测时间更新不使成稿缓存失效");
   var missingOpinion=JsonConvert.DeserializeObject<Newspaper>(JsonConvert.SerializeObject(paper));foreach(var page in missingOpinion.Pages)foreach(var story in page.Articles)story.Genre="消息";handler.Paper=missingOpinion;
   var newsOnly=await service.Generate("missing-opinion","示例市",day,new Preferences(),"fake",CancellationToken.None);Check(newsOnly.Articles.Count>0,"缺少评论不再阻止发行合格新闻");
   handler.Paper=new Newspaper{Headline="交通新变化",Articles=new(){Story("公共交通服务","线路服务已经调整。"),Story("做好服务衔接","交通服务应重视换乘安排。","评论")}};
   var fixedLine=await service.Generate("missing-line","示例市",day,new Preferences(),"fake",CancellationToken.None);Check(fixedLine.Articles.Any(a=>a.Genre=="简讯"&&a.Title.Contains("APM 3 号线投入运营")),"审校未补齐时本地事实工具自动补报新线运营并发行");
   handler.Paper=new Newspaper{Headline="施工消息",Articles=new(){Story("工程进展","施工标记结束，仍待观察。")}};
   var repaired=await service.Generate("bad-copy","示例市",day,new Preferences{QualityCheck=false,AutoRevision=false},"fake",CancellationToken.None);Check(repaired.Articles.Any(a=>a.Body.Contains("APM 3 号线"))&&repaired.Articles.All(a=>!EditionRules.ContainsProcessPhrase(a.Body)),"全是无用稿或关闭可选审校时也能用已确认事实完成发行");
  }
  var output=Path.Combine(project,"artifacts/sample/分版校样.docx");DocxExporter.Save(output,paper);File.WriteAllText(Path.ChangeExtension(output,".json"),JsonConvert.SerializeObject(paper));
  using(var zip=ZipFile.OpenRead(output))using(var reader=new StreamReader(zip.GetEntry("word/document.xml").Open())){var doc=XDocument.Parse(reader.ReadToEnd());XNamespace w="http://schemas.openxmlformats.org/wordprocessingml/2006/main";var text=string.Join("",doc.Descendants(w+"t").Select(x=>x.Value));Check(doc.Descendants(w+"br").Count(x=>(string)x.Attribute(w+"type")=="page")==2&&text.Contains("经济版 · 02")&&text.Contains("评论版 · 03")&&paper.Articles.All(a=>text.Contains(a.Title))&&!text.Contains("{{"),"Word 按主题显式分页，保留版号、评论标签和全部文章");}
 }
}
