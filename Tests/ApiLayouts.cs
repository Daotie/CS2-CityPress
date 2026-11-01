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
static class ApiLayoutTests {
 static void Check(bool value,string message){if(!value)throw new Exception(message);Console.WriteLine("PASS "+message);}
 sealed class Response:HttpMessageHandler {
  public Newspaper Paper;public string Uri,Auth,Body;public int Calls;public bool Fence,Truncate;public HttpStatusCode Code=HttpStatusCode.OK;public JObject Usage;
  protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;Uri=request.RequestUri.AbsoluteUri;Auth=request.Headers.Authorization?.ToString();Body=await request.Content.ReadAsStringAsync();var text=JsonConvert.SerializeObject(Paper);return new HttpResponseMessage(Code){Content=new StringContent(JsonConvert.SerializeObject(new{choices=new[]{new{finish_reason=Truncate?"length":"stop",message=new{content=Fence?"```json\n"+text+"\n```":text}}},usage=Usage}))};}
 }
 static Article Story(string title,string body,string genre="消息")=>new Article{Section=genre=="评论"?"城市评论":"城市",Genre=genre,Title=title,Body=body};
 static string Strip(string text)=>string.Concat(text.Where(c=>!char.IsWhiteSpace(c)));
 public static async Task Run(string temp,string project){
  Check(ApiOptions.Endpoint("https://example.test/v1/").AbsoluteUri=="https://example.test/v1/chat/completions"&&ApiOptions.Endpoint("http://localhost:11434").AbsoluteUri=="http://localhost:11434/v1/chat/completions","API 基础地址和本地服务地址归一化为兼容接口");
  Check(ApiOptions.CredentialId("https://example.test/v1")==ApiOptions.CredentialId("https://example.test/v1/chat/completions/")&&ApiOptions.CredentialId("https://example.test/v1")!=ApiOptions.CredentialId("https://other.test/v1"),"密钥按实际请求地址隔离，同一基础地址与完整地址共用凭据");
  Check(ApiOptions.LegacyDeepSeek(ApiOptions.DefaultEndpoint)&&!ApiOptions.LegacyDeepSeek("https://api.deepseek.com.other.test/v1")&&!ApiOptions.LegacyDeepSeek("http://api.deepseek.com/v1")&&!ApiOptions.LegacyDeepSeek("https://example.test/v1"),"旧 DeepSeek 密钥和环境变量只允许正式 HTTPS DeepSeek 地址使用");
  foreach(var value in new[]{"","file:///C:/data","https://user:password@example.test/v1","https://example.test/v1?key=example","https://example.test/v1#secret"}){bool rejected=false;try{ApiOptions.Endpoint(value);}catch(ArgumentException){rejected=true;}Check(rejected,"拒绝无效或混入凭据的 API 地址");}
  var invalid=new Preferences{ApiEndpoint="invalid",MaxOutputTokens=0};ApiOptions.Repair(invalid);Check(invalid.ApiEndpoint=="invalid"&&invalid.MaxOutputTokens==8192,"修复数值配置不悄悄更换玩家选择的服务地址");
  var day=new DayLog{Day="2027-01-12"};Aggregator.Add(day,"城市实况 · 建筑进入施工","社区医疗中心","10:00");
  var example=new Newspaper{Headline="公共服务的新进展",Deck="城市建设与服务安排",Articles=new(){Story("社区医疗中心建设推进","社区医疗中心建设正在推进。"),Story("服务安排应回应需求","公共服务安排应结合设施分布，统筹建设与维护。","评论")}};
  using(var handler=new Response{Paper=example,Fence=true})using(var service=new NewsService(handler,Path.Combine(temp,"custom-api"))){
   var prefs=new Preferences{ApiEndpoint="http://localhost:11434/v1",Model="local-model",RequireApiKey=false,JsonMode=false,TokenParameter="max_completion_tokens",MaxOutputTokens=12000};
   var result=await service.Generate("local","汐澜",day,prefs,"must-not-send",CancellationToken.None);var body=JObject.Parse(handler.Body);
   Check(handler.Uri=="http://localhost:11434/v1/chat/completions"&&handler.Auth==null&&body["model"].ToString()=="local-model"&&body["response_format"]==null&&body["max_tokens"]==null&&(int)body["max_completion_tokens"]==12000,"自定义地址、模型、无鉴权、JSON 兼容开关与输出参数按配置发送");
   Check(!handler.Body.Contains("must-not-send")&&!handler.Body.Contains("localhost")&&result.Provider=="localhost"&&!result.UsageReported&&!result.CacheUsageReported,"密钥及服务地址不进入提示词，未返回用量不假报缓存");
   Check(result.Pages.Count==2&&result.Pages.Last().Articles.All(a=>a.Genre=="评论")&&result.Pages.All(p=>p.Composition!=null),"独立评论页与版式计划在保存前形成");
   var saved=JsonConvert.SerializeObject(result.Pages.Select(p=>p.Composition));var cached=await service.Generate("local","汐澜",day,prefs,"",CancellationToken.None);Check(cached.LocalCache&&handler.Calls==1&&JsonConvert.SerializeObject(cached.Pages.Select(p=>p.Composition))==saved,"同事实命中本地缓存，阅读与缓存不重新抽取版式");
   prefs.ApiEndpoint="https://second.test/v1";prefs.RequireApiKey=true;prefs.JsonMode=true;prefs.TokenParameter="max_tokens";handler.Usage=JObject.Parse("{\"prompt_tokens\":500,\"completion_tokens\":800,\"prompt_tokens_details\":{\"cached_tokens\":120}}");
   var other=await service.Generate("local","汐澜",day,prefs,"fake-new-provider-key",CancellationToken.None);body=JObject.Parse(handler.Body);Check(handler.Calls==2&&handler.Auth=="Bearer fake-new-provider-key"&&body["response_format"]["type"].ToString()=="json_object"&&other.InputTokens==500&&other.OutputTokens==800&&other.CacheHitTokens==120&&other.CacheUsageReported,"切换服务使缓存隔离，兼容 JSON 模式与通用用量字段");
   prefs.LayoutMode="varied";await service.Generate("local","汐澜",day,prefs,"fake",CancellationToken.None);Check(handler.Calls==3,"版式模式变化使生成缓存失效");
   handler.Code=HttpStatusCode.Redirect;bool redirect=false;try{await service.Generate("redirect","汐澜",day,prefs,"fake",CancellationToken.None);}catch(InvalidOperationException error){redirect=error.Message.Contains("302");}Check(redirect,"接口重定向被拒绝，不自动向其他地址重试");
   handler.Code=HttpStatusCode.OK;handler.Truncate=true;bool truncated=false;try{await service.Generate("truncated","汐澜",day,prefs,"fake",CancellationToken.None);}catch(InvalidOperationException){truncated=true;}Check(truncated&&!Directory.Exists(Path.Combine(temp,"custom-api","cities","truncated","cache")),"截断的长评论稿不缓存，不静默发布半份报纸");
  }
  string paragraph="城市公共服务的安排，需要把设施建设与长期服务放在一起考虑。道路、轨道交通与社区服务相互联系，单项工程的推进应兼顾接驳组织、步行可达性和后续维护。讨论这些选择，应当以明确事实为基础，把建议作为建议提出，不能用期待代替已经实现的效果。";
  var longBody=string.Join("\n",Enumerable.Range(0,9).Select(i=>paragraph));var longStory=Story("从线路建设走向服务协同",longBody,"评论");
  var samples=new List<NewspaperPage>{
   new NewspaperPage{Theme="要闻",Headline="公共服务建设与城市发展",Deck="通栏版式校样",Articles=new(){Story("公共服务建设","社区医疗中心建设推进。"+paragraph),Story("道路施工安排","道路工程与步行通道建设同步安排。"+paragraph),Story("服务资源统筹",paragraph)}},
   new NewspaperPage{Theme="城市",Headline="把公共服务安排落到具体需求",Deck="侧栏主稿版式校样",Articles=new(){Story("把公共服务安排落到具体需求",string.Join("\n",Enumerable.Repeat(paragraph,3))),Story("接驳组织",paragraph),Story("社区服务",paragraph)}},
   new NewspaperPage{Theme="交通",Headline="新线运营服务城市出行",Deck="纵向头条版式校样",Articles=new(){Story("新线运营服务城市出行",paragraph),Story("换乘衔接",paragraph)}},
   new NewspaperPage{Theme="综合",Headline="城市短讯",Deck="综合短讯版式校样",Articles=Enumerable.Range(1,6).Select(i=>Story("城市建设消息"+i,paragraph)).ToList()},
   new NewspaperPage{Theme="评论",Headline=longStory.Title,Deck="长评论专版排版校样",Articles=new(){longStory}}
  };
  var templates=new[]{"banner","lead-left","vertical-lead","digest","opinion"};var sample=new Newspaper{Name="汐澜日报",Day=day.Day,Headline="多版式排版校样",Deck="测试用样张",Pages=samples,Articles=samples.SelectMany(p=>p.Articles).ToList()};
  for(int i=0;i<samples.Count;i++){var page=samples[i];page.Composition=PaperLayout.Compose(page,templates[i]);var plan=page.Composition;var original=string.Join("",page.Articles.Select(a=>a.Body));var reconstructed=string.Join("",plan.Columns.SelectMany(c=>c.Blocks).Where(b=>b.Kind=="body").Select(b=>PaperLayout.BlockText(page,b)));Check(Strip(original)==Strip(reconstructed),templates[i]+" 模板完整保留所有正文与段落顺序");}
  Check(samples[2].Composition.Top.Count==0&&samples[2].Composition.Columns[0].Blocks[0].Kind=="vertical-headline"&&samples[1].Composition.Columns[0].Width>samples[1].Composition.Columns[1].Width,"纵向头条与侧栏主稿不再固定为顶部通栏");
  Check(samples[4].Composition.Top.Count==0&&samples[4].Composition.Columns.Count==3&&samples[4].Composition.Columns.All(c=>c.Blocks.Any(b=>b.Kind=="body")),"长评论正文在专版连续分栏，不重复标题与整篇正文");
  var variants=new HashSet<string>();for(int i=0;i<30;i++){var variable=new Newspaper{Name="校样",Day="2027-01-"+(i+1).ToString("D2"),Pages=new(){JsonConvert.DeserializeObject<NewspaperPage>(JsonConvert.SerializeObject(samples[0]))}};PaperLayout.Assign(variable,"varied");variants.Add(variable.Pages[0].Composition.Template);}Check(variants.Count>=3,"适度随机只选择合适模板，并随不同刊物提供版式变化");
  var auto=new Newspaper{Name="校样",Day=day.Day,Pages=new(){samples[4]}};PaperLayout.Assign(auto,"auto");Check(auto.Pages[0].Composition.Template=="opinion","长评论自动使用专版模板");
  var source=new SaveState{Override=new Preferences{ApiEndpoint="https://configured.test/v1",LayoutMode="varied",LongCommentary=false,JsonMode=false,RequireApiKey=false,MaxOutputTokens=12000}};var snapshot=JournalSnapshot.Capture(source);source.Override.ApiEndpoint="https://changed.test/v1";Check(snapshot.Override.ApiEndpoint=="https://configured.test/v1"&&snapshot.Override.LayoutMode=="varied"&&!snapshot.Override.LongCommentary&&!snapshot.Override.JsonMode&&snapshot.Override.MaxOutputTokens==12000,"日志快照完整保留新增配置，并隔离后续修改");
  var output=Path.Combine(project,"artifacts/sample/多版式校样.docx");DocxExporter.Save(output,sample);File.WriteAllText(Path.ChangeExtension(output,".json"),JsonConvert.SerializeObject(sample));
  using(var zip=ZipFile.OpenRead(output))using(var reader=new StreamReader(zip.GetEntry("word/document.xml").Open())){var doc=XDocument.Parse(reader.ReadToEnd());XNamespace w="http://schemas.openxmlformats.org/wordprocessingml/2006/main";var tables=doc.Descendants(w+"tbl").Where(t=>(string)t.Element(w+"tblPr")?.Element(w+"tblCaption")?.Attribute(w+"val")=="CityPress.Articles").ToArray();Check(tables.Length==5&&doc.Descendants(w+"br").Count(x=>(string)x.Attribute(w+"type")=="page")==4&&!doc.ToString().Contains("{{"),"Word 多模板保留五个主题版与显式分页，无模板占位符");var widths=tables[1].Element(w+"tblGrid").Elements().Select(x=>(int)x.Attribute(w+"w")).ToArray();Check(widths[0]>widths[1]&&tables[2].Descendants(w+"textDirection").Any(x=>(string)x.Attribute(w+"val")=="tbRl"),"Word 输出真实主次栏宽和纵向字列，不只改变版式名称");}
 }
}
