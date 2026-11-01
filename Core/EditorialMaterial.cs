using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CityPress.Core {
 // Pure local editorial tools. Original journals are never changed. Unknown
 // units, prefab identifiers and routine fleet inventories never reach a model.
 public static class EditorialMaterial {
  public static readonly Regex TechnicalCopy=new Regex(@"taxable\s*income|Route\s*Ticket\s*Price|原生分组|分组\s*\d+|Entity\(|[\w\p{IsCJKUnifiedIdeographs}-]+-L\d+-\d+x\d+|非零栅格|最高强度\s*32767",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);
  static string Category(string value) {
   if(Regex.IsMatch(value,"住宅|居住|Residential",RegexOptions.IgnoreCase))return "住宅";
   if(Regex.IsMatch(value,"工业|制造|Industrial",RegexOptions.IgnoreCase))return "工业建筑";
   if(Regex.IsMatch(value,"商业|Commercial",RegexOptions.IgnoreCase))return "商业设施";
   if(Regex.IsMatch(value,"办公|Office",RegexOptions.IgnoreCase))return "办公建筑";
   if(Regex.IsMatch(value,"中学|小学|学校|School",RegexOptions.IgnoreCase))return "学校";
   if(Regex.IsMatch(value,"医疗|诊所|医院|Hospital|Clinic",RegexOptions.IgnoreCase))return "医疗设施";
   if(Regex.IsMatch(value,"地铁|车站|站点|Station",RegexOptions.IgnoreCase))return "交通设施";
   if(Regex.IsMatch(value,"道路|车道|步行|轨道|Road|Track",RegexOptions.IgnoreCase))return "道路与步行设施";
   if(Regex.IsMatch(value,"公园|球场|绿地|Park",RegexOptions.IgnoreCase))return "文体与休闲设施";
   return "城市建筑";
  }
  public static bool TransitChanged(TransitRecord line)=>line!=null&&!string.IsNullOrWhiteSpace(line.Change)&&Regex.IsMatch(line.Change,"投入运营|恢复运营|暂停运营|停运|票价调整");
  public static DayLog Prepare(DayLog source) {
   var result=new DayLog{Day=source.Day};
   foreach(var metric in source.Metrics??new List<MetricRecord>()){
    if(metric?.Key==null)continue;
    var key=metric.Key;
    bool city=Regex.IsMatch(key,@"^city/(population|money|monthlyIncome|monthlyExpenses|monthlyNetIncome|households|tourists|loan)$");
    bool weather=key=="weather/temperature";
    city|=key=="population";
    bool pollution=Regex.IsMatch(key,@"^pollution/[^/]+/affected$");
    if(!city&&!weather&&!pollution)continue;
    var copy=JsonCopy(metric);
    if(pollution){copy.Label=metric.Label.Replace("非零栅格占比","监测范围内检出占比");copy.First*=100;copy.Last*=100;copy.Min*=100;copy.Max*=100;copy.Unit="%";result.Metrics.Add(copy);continue;}
    copy.Unit=key=="population"||key=="city/population"||key=="city/tourists"?"人":key=="city/households"?"户":weather?"摄氏度":"元";
    result.Metrics.Add(copy);
   }
   result.Breakthroughs=(source.Breakthroughs??new List<BreakthroughRecord>()).Where(x=>x!=null&&x.Key!="city/monthlyBalance"&&x.Key!="city/ownedTiles").Select(x=>{var copy=JsonCopy(x);copy.Unit=x.Key=="city/population"||x.Key=="city/tourists"?"人":x.Key=="city/households"?"户":"元";return copy;}).ToList();
   result.Transit=(source.Transit??new List<TransitRecord>()).Where(TransitChanged).Select(TransitFacts.Copy).ToList();
   var eventLatest=new Dictionary<string,ActionRecord>();
   foreach(var action in EditorialInput.Select(source)){
    if(action.Kind.Contains("城市事件")){
     var id=Regex.Match(action.Detail,@"事件档案=(Entity\(\d+:\d+\))");
     string key=id.Success?id.Groups[1].Value:action.Detail;
     if(!eventLatest.TryGetValue(key,out var old)||string.CompareOrdinal(action.Last,old.Last)>=0)eventLatest[key]=action;
     continue;
    }
    if(Regex.IsMatch(action.Kind+" "+action.Detail,"设置变化|票价工具|Route Ticket Price|施工结束|工具提交|施工标记|火情标记|建筑类型变更|公共交通线路"))continue;
    if(action.Kind.Contains("树木"))continue;
    string detail=action.Detail;
    if(action.Kind.Contains("城市实况")||TechnicalCopy.IsMatch(detail))detail=Category(detail);
    string kind=action.Kind.Replace("城市实况 · ","").Replace("实体（运营状态未核实）","").Replace("路网区段（非整条道路）","路网调整");
    Aggregator.Add(result,kind,detail,"");
    result.Actions.Last(x=>x.Kind==kind&&x.Detail==detail).Count+=action.Count-1;
   }
   foreach(var action in eventLatest.Values){
    var casualties=Regex.Match(action.Detail,@"Casualties=(\d+)");
    // Low-impact routine incidents do not deserve a safety page. Damages is
    // an uncalibrated internal counter, never an amount of property loss.
    if(!casualties.Success||!int.TryParse(casualties.Groups[1].Value,out var count)||count==0)continue;
    string subject=Regex.IsMatch(action.Detail,"火灾|Fire",RegexOptions.IgnoreCase)?"火灾":"事故";
    result.Actions.Add(new ActionRecord{Kind="公共安全事件",Detail=subject+"，伤亡人数"+count+"人"+(action.Detail.Contains("事件已结束")?"，处置结束":""),Count=1});
   }
   foreach(var service in new[]{"electricity","water","sewage"}){
    var need=(source.Metrics??new List<MetricRecord>()).FirstOrDefault(m=>m.Key==service+"/consumption");
    var served=(source.Metrics??new List<MetricRecord>()).FirstOrDefault(m=>m.Key==service+"/fulfilled");
    if(need==null||served==null||need.Last<=0||served.Last>=need.Last*.99)continue;
    result.Actions.Add(new ActionRecord{Kind="市政供给变化",Detail=(service=="electricity"?"供电":service=="water"?"供水":"污水处理")+"出现供需缺口",Count=1});
   }
   return result;
  }
  static T JsonCopy<T>(T source)=>Newtonsoft.Json.JsonConvert.DeserializeObject<T>(Newtonsoft.Json.JsonConvert.SerializeObject(source));
  public static string Display(double value,string unit) {
   double amount=Math.Abs(value);string number=amount>=100000000?(value/100000000).ToString("0.##",CultureInfo.InvariantCulture)+"亿":amount>=10000?(value/10000).ToString("0.##",CultureInfo.InvariantCulture)+"万":value.ToString("0.##",CultureInfo.InvariantCulture);
   return number+unit;
  }
  public static void CleanFurniture(Newspaper paper) {
   bool Unsafe(string text)=>TechnicalCopy.IsMatch(text??"")||Regex.IsMatch(text??"",@"\d{7,}")||EditionRules.ContainsProcessPhrase(text);
   foreach(var page in PagePlanner.PagesOf(paper)){
    if(Unsafe(page.Headline))page.Headline=page.Articles.FirstOrDefault()?.Title??page.Theme;
    if(Unsafe(page.Deck))page.Deck="";
   }
   if(Unsafe(paper.Headline))paper.Headline=PagePlanner.PagesOf(paper).FirstOrDefault()?.Headline??paper.Name;
   if(Unsafe(paper.Deck))paper.Deck="";
  }
  public const string Guidance=@"成稿必须经过取舍：不把素材清单改写成报纸。建设按住宅、产业与公共服务等实际变化归并成少量连贯报道，不逐资产、规格、地块和道路段落列账；同类新增和拆除综合叙述，工具次数不能充作工程量。社会公共安全只写有实质影响的事件，不报道零伤亡、无明确影响的日常小事故，不列预制体与资产名，不把Damages计数称作财产损失。交通仅报道本期明确的开通、恢复、停运和实质票价变化，既有线路清单、车辆数、站台数及常规运行不另成稿；停靠点不是车站数，票价没有原价不写涨跌。不能凭借票价工具或设置刷新评论治理得失。原始税收分组、无单位电水数值、储能内部数值、污染强度原始量纲均不刊发。金额和人口用万、亿与适度小数，不堆未经整理的长整数。供需正常时不反复报道常规满足需求。不要向读者讲解存量流量、数据口径、不可相加等制作核实过程。评论聚焦真实公共议题，多个有事实支撑的议题可以各有长评或短评，不用一篇财政口径辨析代替全部评论。要闻只提炼重要主线和其他专题的核心变化，每个导读最多一句；小事仅在专题版刊发。具体详见版号由排版程序生成，不自行猜编号。";
 }
}
