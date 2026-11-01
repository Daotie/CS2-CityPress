using System;
using System.Collections.Generic;
using System.Linq;
using CityPress.Core;
using Newtonsoft.Json;

static class MaterialTests {
 static void Check(bool value,string message){if(!value)throw new Exception(message);Console.WriteLine("PASS "+message);}
 public static void Run(){
  var raw=new DayLog{Day="2027-01-12"};
  Aggregator.Add(raw,"城市实况 · 建筑新增","中国中层住宅01-L5-6x6","10:00");
  Aggregator.Add(raw,"城市实况 · 建筑新增","中国中层住宅01-L5-4x4","11:00");
  Aggregator.Add(raw,"城市实况 · 建筑进入施工","工业制造01-L1-5x6","11:00");
  Aggregator.Add(raw,"城市事件（累计观测，不得重复相加）","事件档案=Entity(1:1)；Lose Control Accident；Casualties=0；事件已结束","10:00");
  Aggregator.Add(raw,"城市事件（累计观测，不得重复相加）","事件档案=Entity(2:1)；建筑火灾；Damages=1；Casualties=0；事件已结束","10:01");
  Aggregator.Add(raw,"城市事件（累计观测，不得重复相加）","事件档案=Entity(3:1)；建筑火灾；Casualties=1；事件记录中","10:02");
  Aggregator.Add(raw,"城市事件（累计观测，不得重复相加）","事件档案=Entity(3:1)；建筑火灾；Casualties=2；事件已结束","10:03");
  Aggregator.Sample(raw,"native/TaxableIncome/3","工业制造 taxable income（分组3）","未知单位",63249024,"10:00");
  Aggregator.Sample(raw,"electricity/production","供电能力","原生电力单位",9574214,"10:00");
  Aggregator.Sample(raw,"city/monthlyIncome","月度预算总收入","原生单位",105899335,"10:00");
  raw.Transit.Add(new TransitRecord{Id="old",Name="地铁1号线",Operating=true,Stops=28,Vehicles=17});
  raw.Transit.Add(new TransitRecord{Id="new",Name="APM-3",Operating=true,Stops=8,Vehicles=3,Change="投入运营"});
  string before=JsonConvert.SerializeObject(raw);var prepared=EditorialMaterial.Prepare(raw);string text=JsonConvert.SerializeObject(prepared);
  Check(before==JsonConvert.SerializeObject(raw),"素材整理不修改本地原始记录");
  Check(prepared.Actions.Single(a=>a.Detail=="住宅").Count==2&&!text.Contains("L5-6x6")&&!text.Contains("taxable"),"资产规格归并成建筑类别，未知税收分组不送入编辑");
  Check(prepared.Actions.Count(a=>a.Kind=="公共安全事件")==1&&text.Contains("伤亡人数2人")&&!text.Contains("Damages")&&!text.Contains("Entity("),"小事故舍弃，重大事件采用最后累计伤亡值且不泄露资产编号");
  Check(prepared.Transit.Count==1&&prepared.Transit[0].Name=="APM-3"&&prepared.Metrics.Count==1,"既有交通清单和未知电水量纲剔除，明确的新线开通与财政事实保留");
  Check(EditorialMaterial.Display(105899335,"元")=="1.06亿元"&&EditorialMaterial.Display(410118,"人")=="41.01万人","人口金额统一整理单位和精度");
  Check(!PaperLayout.VerticalEligible("市库资金余额突破1800万 常住人口达41.01万")&&PaperLayout.VerticalEligible("新线开通服务城市出行"),"数字与长复合标题不进入竖排");
  var legacy=new NewspaperPage{Headline="市库资金突破1800万",Articles=new List<Article>()};legacy.Composition=new PageComposition{Template="vertical-lead"};
  Check(PaperLayout.Composition(legacy).Template=="banner","旧报纸中的不适合竖排标题也回退到横排");
  var finance=new Article{Section="财政",Genre="消息",Title="财政安排",Body=string.Concat(Enumerable.Repeat("市库余额上升，公共投入应结合服务需求安排。",25))};
  var minor=new Article{Section="生态",Genre="简讯",Title="气温变化",Body="气温回落。"};
  var paper=new Newspaper{Headline="城市发展与公共服务",Pages=new List<NewspaperPage>{new NewspaperPage{Theme="要闻",Articles=new List<Article>{new Article{Title="人口增长",Body="人口增长。"}}},new NewspaperPage{Theme="经济与财政",Articles=new List<Article>{finance}},new NewspaperPage{Theme="环境与观察",Articles=new List<Article>{minor}}}};
  PagePlanner.AddFrontPageGuide(paper,new EditorialBrief());PagePlanner.AddFrontPageGuide(paper,new EditorialBrief());
  var guide=paper.Pages[0].Articles.Single(a=>a.Section=="本期导读");
  Check(guide.Body.Contains("详见第2版")&&!guide.Body.Contains("气温")&&paper.Pages[0].Articles.Count==2,"导读引用最终版号且重复生成不累加，小事只保留在专题版");
  var bad=new Article{Genre="消息",Title="产业变化",Body="工业制造01-L1-5x6进入施工，taxable income分组3达到63249024，相关工业地块活动较为集中。"};
  var unsafePaper=new Newspaper{Headline="产业",Articles=new List<Article>{bad}};
  Check(QualityControl.Review(unsafePaper,new EditorialBrief(),prepared).HasFatal,"即使模型仍照抄原始字段与资产编码，成稿检查也会拦截");
  unsafePaper.Pages=new List<NewspaperPage>{new NewspaperPage{Theme="产业",Headline="工业制造01-L1-5x6进入施工",Deck="taxable income分组3增加",Articles=new List<Article>{new Article{Title="工业建筑进入施工",Body="工业建筑进入施工。"}}}};
  EditorialMaterial.CleanFurniture(unsafePaper);
  Check(unsafePaper.Pages[0].Headline=="工业建筑进入施工"&&unsafePaper.Pages[0].Deck=="","头条与导语同样不能泄露资产编码和未知字段");
  var longArticles=Enumerable.Range(0,3).Select(i=>new Article{Genre="评论",Title="议题"+i,Body=new string('论',2000)}).ToList();
  var opinions=new Newspaper{Headline="评论专刊",Articles=longArticles};PagePlanner.Plan(opinions,new EditorialBrief());
  Check(opinions.Pages.Count==3&&opinions.Articles.Count==3,"评论超出容量时支持连续加版，不把第二篇长评硬塞进同一版");
 }
}
