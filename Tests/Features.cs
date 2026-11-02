using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CityPress.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
static class FeatureTests {
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);Console.WriteLine("PASS "+message);}
 public static async Task Run(string root) {
  Check(ExportPaths.DefaultDirectory==Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"CityPress"),"默认导出根目录跟随系统文档位置（包括重定向文档）");
  var export=Path.Combine(root,"export folder 中文");var safe=ExportPaths.Folder(export,"汐澜/日报","save-a");
  Check(safe.StartsWith(Path.GetFullPath(export)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)&&!ExportPaths.Segment("CON.txt").Equals("CON.txt")&&ExportPaths.Segment("A/B")!=ExportPaths.Segment("A?B"),"报纸目录处理非法/保留名称，避免清洗后重名且不越出根目录");
  bool relative=false;try{ExportPaths.Root("../newspapers");}catch(ArgumentException){relative=true;}Check(relative,"拒绝相对导出路径，避免写入意外工作目录");
  Check(ExportPaths.Folder(export,"同名日报","save-a")!=ExportPaths.Folder(export,"同名日报","save-b"),"同名报纸在不同存档中分开保存");
  var paper=new Newspaper{Name="汐澜/日报",Day="2027-01-12",Headline="城市新闻",Deck="公开报道",Articles=new System.Collections.Generic.List<Article>{new Article{Section="城市",Title="人口增长",Body="常住人口已达到新的规模。"}}};
  var store=new ExportStore(Path.Combine(root,"data"));var saved=store.Save("save-a",paper,export,CancellationToken.None);
  Check(File.Exists(Path.Combine(saved.Folder,paper.Day+".docx"))&&File.Exists(Path.Combine(saved.Folder,paper.Day+".json")),"生成后的 Word 与 JSON 导出到自定义目录并按报纸名称分类");
  var changed=Path.Combine(root,"new-export");Check(store.Read("save-a",paper.Day,changed).Folder==saved.Folder,"改根目录后，历史报纸仍打开实际原存储位置");
  var copied=store.Save("save-a",paper,changed,CancellationToken.None);Check(copied.Folder!=saved.Folder&&File.Exists(Path.Combine(saved.Folder,paper.Day+".docx"))&&store.List("save-a").Length==1,"重新导出保留旧文件，更新位置但不重复档案条目");
  // Simulate Word holding the prior issue open without delete sharing.
  var target=Path.Combine(copied.Folder,paper.Day+".docx");var originalBytes=File.ReadAllBytes(target);
  using(var locked=new FileStream(target,FileMode.Open,FileAccess.Read,FileShare.Read)){
   var alternate=store.Save("save-a",paper,changed,CancellationToken.None);
   Check(alternate.ExportRenamed&&!alternate.ExportDeferred&&alternate.Paper==paper&&Directory.GetFiles(alternate.Folder,paper.Day+"-*.docx").Length==1,"Word 占用旧文件时自动另存新副本，界面仍得到成稿");
  }
  Check(File.ReadAllBytes(target).SequenceEqual(originalBytes),"被占用的原 Word 文件保持完整");
  var blocked=Path.Combine(root,"blocked-export");File.WriteAllText(blocked,"file blocks directory creation");
  var fallback=store.Save("save-a",paper,blocked,CancellationToken.None);
  Check(fallback.ExportRedirected&&!fallback.ExportDeferred&&fallback.Folder==store.Issues("save-a")&&File.Exists(Path.Combine(fallback.Folder,paper.Day+".docx")),"自定义目录不可用时自动保存到本地归档并返回可阅读成稿");
  var unavailable=new ExportStore(Path.Combine(blocked,"unwritable"));var memory=unavailable.Save("save-a",paper,blocked,CancellationToken.None);
  Check(memory.ExportDeferred&&memory.ArchiveDeferred&&memory.Paper==paper,"所有保存位置失败时仍返回内存成稿供游戏阅读与重新导出");
  var legacy=Path.Combine(root,"data","cities","legacy","issues");Directory.CreateDirectory(legacy);Storage.Write(Path.Combine(legacy,paper.Day+".json"),JsonConvert.SerializeObject(paper));File.Copy(Path.Combine(saved.Folder,paper.Day+".docx"),Path.Combine(legacy,paper.Day+".docx"));
  Check(store.Read("legacy",paper.Day,export).Folder==legacy,"旧版 AppData 报纸可继续打开原文件夹");
  using(var token=new CancellationTokenSource()){token.Cancel();bool canceled=false;try{store.Save("canceled",paper,export,token.Token);}catch(OperationCanceledException){canceled=true;}Check(canceled&&!Directory.Exists(store.Issues("canceled")),"切换会话取消的导出不会发布新档案");}
  var state=new SaveState();var day=new DayLog{Day="2027-01-12"};state.Days.Add(day);
  Check(!Milestones.Observe(state,day,"city/population",388000,"08:00")&&day.Breakthroughs.Count==0,"首次采样建立人口基线，不把安装前已有规模当新突破");
  Check(Milestones.Observe(state,day,"city/population",400100,"09:00")&&day.Breakthroughs.Single().Threshold==400000,"常住人口跨过 40 万时生成重大突破素材");
  Milestones.Observe(state,day,"city/population",451000,"10:00");Check(day.Breakthroughs.Count==1&&day.Breakthroughs[0].Threshold==450000,"同一指标同日多次越线合并为最高一级");
  var restored=JsonConvert.DeserializeObject<SaveState>(JsonConvert.SerializeObject(state));var next=new DayLog{Day="2027-01-13"};
  Check(!Milestones.Observe(restored,next,"city/population",420000,"08:00")&&!Milestones.Observe(restored,next,"city/population",450000,"09:00")&&next.Breakthroughs.Count==0,"高位随存档保留，回落、反弹与读档不会重复报突破");
  Check(Milestones.Observe(restored,next,"city/population",1000001,"10:00")&&next.Breakthroughs.Count==1&&next.Breakthroughs[0].Threshold==1000000,"一次跨过多个数量级只记录最高突破");
  Check(!Milestones.Observe(state,day,"native/Unknown/0",99999999,"10:00")&&!Milestones.Observe(state,day,"city/money",double.NaN,"10:00"),"未解释的指标与无效数值不伪造重大突破");
  Milestones.Observe(state,day,"city/monthlyIncome",0,"08:00");Check(!Milestones.Observe(state,day,"city/monthlyIncome",5000000,"09:00"),"财政面板初始化的零值不冒充有效月收入基线");
  Milestones.Observe(state,day,"city/monthlyIncome",6100000,"10:00");Milestones.Observe(state,day,"city/money",1400000000,"10:00");Milestones.Observe(state,day,"city/money",1510000000,"11:00");
  Check(day.Breakthroughs.Any(x=>x.Key=="city/monthlyIncome"&&x.Unit.Contains("/月"))&&day.Breakthroughs.Any(x=>x.Key=="city/money"&&x.Threshold==1500000000),"月收入和市库存量独立检测，支持十亿级资金");
  Milestones.Observe(state,day,"city/monthlyNetIncome",-300000,"11:00");Check(Milestones.Observe(state,day,"city/monthlyNetIncome",10000,"12:00")&&day.Breakthroughs.Any(x=>x.Kind=="预算由亏转盈"),"月度预算由亏转盈有独立事实类型");
  var handler=new Fake();using(var service=new NewsService(handler,Path.Combine(root,"news"))){
   var prefs=new Preferences{ExportDirectory=export};await service.Generate("test","汐澜",day,prefs,"test",CancellationToken.None);
   var payload=JObject.Parse(handler.Last);var input=JObject.Parse((string)payload["messages"][1]["content"]);
   Check(input["breakthroughs"].Count()==day.Breakthroughs.Count&&!input.ToString().Contains("export folder 中文")&&handler.Calls==1,"突破事实进入编辑素材并触发新闻，导出路径不发送给 DeepSeek");
   await service.Generate("test","汐澜",day,prefs,"",CancellationToken.None);Check(handler.Calls==1,"含突破的同一事实仍命中本地缓存");
  }
  var indexed=new DayLog{Day="2027-01-12"};for(int i=0;i<10000;i++){Aggregator.Add(indexed,"建设","设施"+i,"10:00");Aggregator.Sample(indexed,"metric"+i,"指标","",i,"10:00");}for(int i=0;i<10000;i++)Aggregator.Add(indexed,"建设","设施"+i,"11:00");
  Check(Aggregator.Count(indexed)==20000&&indexed.Actions.Count==10000&&indexed.Metrics.Count==10000,"大批量操作与指标使用索引，合并计数仍准确");
  var source=new SaveState{Override=new Preferences{ExportDirectory=export}};source.Days.Add(new DayLog{Day="2027-01-11"});source.Days.Add(indexed);source.MilestoneProgress["city/population"]=new MilestoneProgress{High=450000};
  var snapshot=JournalSnapshot.Capture(source);Aggregator.Add(indexed,"建设","设施0","12:00");source.Override.ExportDirectory=changed;source.MilestoneProgress["city/population"].High=500000;
  Check(snapshot.Days[1].Actions[0].Count==2&&snapshot.Override.ExportDirectory==export&&snapshot.MilestoneProgress["city/population"].High==450000,"后台日志快照隔离当前可变记录、设置与突破高位");
  var journals=new JournalStore(Path.Combine(root,"journal-data"));journals.Write(snapshot,CancellationToken.None);var oldPath=Path.Combine(root,"journal-data","cities",source.Id,"journals","2027-01-11.json");var stamp=File.GetLastWriteTimeUtc(oldPath);
  journals.Write(JournalSnapshot.Capture(source),CancellationToken.None);var read=journals.Read(source.Id);
  Check(File.GetLastWriteTimeUtc(oldPath)==stamp&&read.Days.Last().Actions[0].Count==3,"按天日志不反复重写历史，当前记录可恢复");
  var slice=new WorkSlice(3,10000);int rows=0;while(slice.Take())rows++;Check(rows==3,"采集切片有明确数量上限，不在一帧跑完整个扫描");
 }
}
