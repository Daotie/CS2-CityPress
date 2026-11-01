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
  var paper=new Newspaper{Name="汐澜日报",Day=day.Day,Headline="轨道交通与公共服务",Deck="排版测试样张",Pages=new(){
   new NewspaperPage{Theme="要闻",Headline="APM 3 号线投入运营",Deck="地铁服务增添新线路",Articles=new(){Story("APM 3 号线投入运营","APM 3 号线已投入运营，设有8个停站，票价为6元。\n线路服务与市民出行密切相关。"),Story("人口跨过四十万","常住人口达到40.46万。新增公共服务需求应纳入城市规划。")}},
   new NewspaperPage{Theme="经济",Headline="月度预算保持结余",Deck="统筹财政资源与公共服务",Articles=new(){Story("月度预算结余3700万元","月度预算收入为1.05亿元，支出为6800万元，结余3700万元。\n市库余额为1500.8万元。"),Story("让预算安排回应服务需求","人口规模扩大，公共服务的投入方向值得重视。预算安排应结合学校、医疗与交通服务需求，统筹建设和后续支出。","分析")}},
   new NewspaperPage{Theme="评论",Headline="新线开通之后还要做好服务衔接",Deck="围绕城市交通的短评",Articles=new(){Story("把线路延伸变成出行便利","APM 3 号线投入运营，为讨论公共交通服务衔接提供了新的切入点。\n一条线路的价值需要通过换乘组织、站点可达性和服务稳定性体现。后续管理宜关注接驳安排，在有实际客流依据后优化运力。","评论"),Story("财政安排需要长期眼光","本期月度预算保持结余，给城市公共服务的安排提供了讨论基础。\n资金安排应把建设与维护一并考虑。对服务需求的判断还应结合人口与设施分布，让预算选择有明确的公共目标。","评论")}}
  }};
  EditionRules.Normalize(paper);Check(paper.Articles.Count==6&&PaperLayout.Pages(paper).Count==3,"分版稿按版面保留顺序，同时生成兼容的完整文章列表");
  Check(PaperLayout.Pages(new Newspaper{Headline="旧刊",Articles=new(){Story("历史消息","正文")}}).Single().Theme=="综合","旧单版报纸无需迁移即可阅读与导出");
  var oldPrefs=new Preferences{Editorial=EditorialPolicy.PriorProfessional};Check(EditorialPolicy.Upgrade(oldPrefs)&&oldPrefs.Editorial==EditorialPolicy.Default,"beta.4默认编辑方针自动升级，保留自定义设置机制");
  using(var handler=new Response{Paper=paper})using(var service=new NewsService(handler,Path.Combine(temp,"editions"))){
   Aggregator.Sample(day,"city/monthlyBalance","旧错误结余","",173000000,"18:00");day.Breakthroughs.Add(new BreakthroughRecord{Key="city/monthlyBalance",Value=173000000});
   var result=await service.Generate("edition","汐澜",day,new Preferences(),"fake",CancellationToken.None);var request=JObject.Parse(handler.Request);var facts=JObject.Parse((string)request["messages"][1]["content"]);
   Check(facts["transit"][0]["Name"].ToString()=="APM 3 号线"&&(bool)facts["transit"][0]["Operating"]&&facts["metrics"].Count()==0&&facts["breakthroughs"].Count()==0,"线路原名与实际服务进入素材，旧错误财政指标及突破隔离");
   repeated.Time="18:40";repeated.Name="APM 3 号线";TransitFacts.Store(day,repeated);await service.Generate("edition","汐澜",day,new Preferences(),"",CancellationToken.None);Check(handler.Calls==1,"普通线路观测时间更新不使成稿缓存失效");
   var missingOpinion=JsonConvert.DeserializeObject<Newspaper>(JsonConvert.SerializeObject(paper));foreach(var page in missingOpinion.Pages)foreach(var story in page.Articles)story.Genre="消息";handler.Paper=missingOpinion;bool opinionRejected=false;try{await service.Generate("missing-opinion","汐澜",day,new Preferences(),"fake",CancellationToken.None);}catch(InvalidOperationException error){opinionRejected=error.Message.Contains("评论或分析");}Check(opinionRejected,"重大线路新闻缺少评论或分析时拒绝成稿");
   handler.Paper=new Newspaper{Headline="交通新变化",Articles=new(){Story("公共交通服务","线路服务已经调整。"),Story("做好服务衔接","交通服务应重视换乘安排。","评论")}};bool nameRejected=false;try{await service.Generate("missing-line","汐澜",day,new Preferences(),"fake",CancellationToken.None);}catch(InvalidOperationException error){nameRejected=error.Message.Contains("新线运营");}Check(nameRejected,"已确认的新线运营消息不能省略线路原名");
   handler.Paper=new Newspaper{Headline="施工消息",Articles=new(){Story("工程进展","施工标记结束，仍待观察。")}};bool rejected=false;try{await service.Generate("bad-copy","汐澜",day,new Preferences(),"fake",CancellationToken.None);}catch(InvalidOperationException){rejected=true;}Check(rejected&&!Directory.Exists(Path.Combine(temp,"editions","cities","bad-copy","cache")),"含技术口径和模板免责措辞的稿件拒绝缓存，不伪造事实替换");
  }
  var output=Path.Combine(project,"artifacts/sample/分版校样.docx");DocxExporter.Save(output,paper);File.WriteAllText(Path.ChangeExtension(output,".json"),JsonConvert.SerializeObject(paper));
  using(var zip=ZipFile.OpenRead(output))using(var reader=new StreamReader(zip.GetEntry("word/document.xml").Open())){var doc=XDocument.Parse(reader.ReadToEnd());XNamespace w="http://schemas.openxmlformats.org/wordprocessingml/2006/main";var text=string.Join("",doc.Descendants(w+"t").Select(x=>x.Value));Check(doc.Descendants(w+"br").Count(x=>(string)x.Attribute(w+"type")=="page")==2&&text.Contains("经济版 · 02")&&text.Contains("评论版 · 03")&&paper.Articles.All(a=>text.Contains(a.Title))&&!text.Contains("{{"),"Word 按主题显式分页，保留版号、评论标签和全部文章");}
 }
}
