using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CityPress.Core {
 // Step 2/3 of the editorial process: turn raw observations into valued events.
 // The AI still decides the wording; this layer decides what deserves space,
 // which event may support a commentary and which facts the checks can verify.
 public enum NewsImportance { Routine=0, Notable=1, Major=2, Milestone=3, Critical=4 }
 public sealed class NewsEvent {
  public string Id, Kind, Subject, Domain, Detail;
  public NewsImportance Importance;
  public List<string> Facts=new List<string>();
  public bool IsBreakthrough;
  public double Threshold;
  public string MajorToken;
 }
 public sealed class EditorialBrief {
  public List<NewsEvent> Events=new List<NewsEvent>();
  public bool CommentaryWarranted;
  public string CommentaryReason="";
  public string CommentaryFocus="";
  public bool HasMajor=>Events.Any(x=>x.Importance>=NewsImportance.Major);
  public IEnumerable<NewsEvent> MajorEvents=>Events.Where(x=>x.Importance>=NewsImportance.Major);
  public NewsEvent Top=>Events.OrderByDescending(x=>x.Importance).ThenByDescending(x=>x.Facts.Count).FirstOrDefault();
  public bool MatchesMajor(string text) {
   if(string.IsNullOrWhiteSpace(text))return false;
   return MajorEvents.Any(x=>!string.IsNullOrEmpty(x.MajorToken)&&text.Contains(x.MajorToken));
  }
  public string FactSheet(){
   var lines=Events.Where(x=>x.Importance>=NewsImportance.Notable).OrderByDescending(x=>x.Importance).Take(24)
    .SelectMany(x=>x.Facts.Take(3).Select(f=>"- "+f)).ToList();
   if(lines.Count>60)lines=lines.Take(60).ToList();
   return string.Join("\n",lines);
  }
 }
 public static class NewsValue {
  static readonly CultureInfo Inv=CultureInfo.InvariantCulture;
  public static bool IsReportableMetric(MetricRecord metric) {
   if(metric?.Key==null)return false;
   if(metric.Key=="city/monthlyBalance")return false;
   if(metric.Key.StartsWith("native-sample/",StringComparison.Ordinal))return false;
   return metric.Min!=metric.Max||metric.Key.StartsWith("city/",StringComparison.Ordinal)||metric.Key=="native/Population/0";
  }
  static string DomainOf(string kind,string detail) {
   var text=(kind??"")+" "+(detail??"");
   if(new[]{"财政","预算","收入","支出","结余","税","贷款","市库","资金","经济","商业","产业","就业","失业"}.Any(text.Contains))return "经济与财政";
   if(new[]{"交通","公交","地铁","轨道","线路","车站","铁路","轮渡","航空","道路","施工","建设","设施","建筑","用地","拆除","路网"}.Any(text.Contains))return "城市建设与交通";
   if(new[]{"污染","环境","空气","土壤","噪声","天气","气温","降雨","降雪","绿化"}.Any(text.Contains))return "环境与观察";
   if(new[]{"人口","家庭","民生","教育","学校","医疗","医院","住宅","住房","游客","住宿","社区","服务","福利"}.Any(text.Contains))return "民生与社会";
   if(new[]{"火灾","火情","事故","损毁","废弃","治安","安全","犯罪","伤亡"}.Any(text.Contains))return "社会";
   return "城市要闻";
  }
  static NewsImportance BreakthroughImportance(BreakthroughRecord record) {
   double threshold=Math.Max(0,record.Threshold);
   switch(record.Key){
    case "city/money":return threshold>=10000000?NewsImportance.Milestone:NewsImportance.Major;
    case "city/population":return threshold>=100000?NewsImportance.Milestone:NewsImportance.Major;
    case "city/households":return threshold>=100000?NewsImportance.Milestone:NewsImportance.Major;
    case "city/tourists":return threshold>=50000?NewsImportance.Milestone:NewsImportance.Major;
    case "city/monthlyIncome":case "city/monthlyNetIncome":return threshold>=100000000?NewsImportance.Milestone:NewsImportance.Major;
    case "city/ownedTiles":return threshold>=100?NewsImportance.Milestone:NewsImportance.Major;
    default:return NewsImportance.Major;
   }
  }
  static string Short(double value) {
   double absolute=Math.Abs(value);
   if(absolute>=100000000)return (value/100000000).ToString("0.##",Inv)+"亿";
   if(absolute>=10000)return (value/10000).ToString("0.##",Inv)+"万";
   return value.ToString("0.##",Inv);
  }
  public static EditorialBrief Evaluate(DayLog day,string city) {
   var brief=new EditorialBrief();if(day==null)return brief;
   var breakthroughs=(day.Breakthroughs??new List<BreakthroughRecord>()).Where(x=>x!=null&&x.Key!="city/monthlyBalance").ToList();
   foreach(var record in breakthroughs){
    var label=string.IsNullOrWhiteSpace(record.Label)?record.Key:record.Label;
    var token=!string.IsNullOrWhiteSpace(record.Label)&&record.Label.Length>=3?record.Label:label;
    var severity=BreakthroughImportance(record);
    brief.Events.Add(new NewsEvent{Id="breakthrough:"+record.Key,Kind=record.Kind??"数量级突破",Subject=label,Domain=DomainOf(label,record.Unit),Importance=severity,IsBreakthrough=true,Threshold=record.Threshold,MajorToken=token,
     Facts=new List<string>{
      label+"越过阈值 "+Short(record.Threshold)+rest(record.Unit),
      label+"此前高位 "+Short(record.PreviousHigh)+"，本次观测值 "+Short(record.Value)+rest(record.Unit),
      "同日观测到的越级变化（"+label+"），只按观测范围内的事实报道"
     }});
   }
   // More than one milestone in one day is itself an exceptional news day.
   var majorBreakthroughs=brief.Events.Where(x=>x.IsBreakthrough).OrderByDescending(x=>x.Importance).ToList();
   if(majorBreakthroughs.Count>=3&&majorBreakthroughs[0].Importance<NewsImportance.Critical)majorBreakthroughs[0].Importance=NewsImportance.Critical;
   foreach(var line in (day.Transit??new List<TransitRecord>()).Where(x=>x!=null&&!string.IsNullOrWhiteSpace(x.Name)&&!string.IsNullOrWhiteSpace(x.Change))){
    bool opening=line.Change.Contains("投入运营")||line.Change.Contains("恢复运营");
    var lineFacts=new List<string>{line.Mode+"线路「"+line.Name+"」"+line.Change};
    if(line.Stops>0)lineFacts.Add("「"+line.Name+"」共 "+line.Stops+" 个去重停站或站台");
    if(line.Vehicles>0)lineFacts.Add("「"+line.Name+"」观测到 "+line.Vehicles+" 辆实际服务车辆");
    if(line.Fare>0)lineFacts.Add("「"+line.Name+"」票价为 "+line.Fare);
    brief.Events.Add(new NewsEvent{Id="transit:"+line.Id,Kind=opening?"线路开通":"线路变化",Subject=line.Name,Domain="城市建设与交通",Importance=opening?NewsImportance.Major:NewsImportance.Notable,Facts=lineFacts,MajorToken=line.Name});
   }
   foreach(var action in EditorialInput.Select(day)) {
    var domain=DomainOf(action.Kind,action.Detail);
    var importance=NewsImportance.Routine;
    var text=(action.Kind??"")+" "+(action.Detail??"");
    if(domain=="社会"&&Regex.IsMatch(text,"死亡|伤亡|重伤"))importance=NewsImportance.Major;
    else if(domain=="社会")importance=NewsImportance.Notable;
    else if(domain=="经济与财政")importance=Regex.IsMatch(text,"预算|结余|收入|支出|贷款|税")?NewsImportance.Notable:NewsImportance.Routine;
    else if(domain=="城市建设与交通")importance=action.Count>=2||Regex.IsMatch(text,"线路|车站|用地|政策|升级")?NewsImportance.Notable:NewsImportance.Routine;
    else importance=action.Count>=2?NewsImportance.Notable:NewsImportance.Routine;
    if(importance==NewsImportance.Routine)continue;
    brief.Events.Add(new NewsEvent{Id="action:"+Storage.Hash(action.Kind+"|"+action.Detail).Substring(0,10),Kind="城市动态",Subject=action.Detail,Domain=domain,Importance=importance,
     Facts=new List<string>{action.Kind+(action.Count>1?"，同类记录合计 "+action.Count+" 次":"")+"；对象："+Truncate(action.Detail,120)}});
   }
   var reportable=(day.Metrics??new List<MetricRecord>()).Where(IsReportableMetric).ToList();
   var changed=reportable.Where(x=>x.Samples>1&&Math.Abs(x.Last-x.First)>0.0000001)
    .Select(x=>new{Metric=x,Relative=x.First==0?0:Math.Abs(x.Last-x.First)/Math.Abs(x.First)})
    .Where(x=>x.Relative>=0.05||(x.Metric.Key=="city/population"&&Math.Abs(x.Metric.Last-x.Metric.First)>=500))
    .OrderByDescending(x=>x.Relative).Take(5).ToList();
   foreach(var item in changed) {
    var metric=item.Metric;var domain=DomainOf(metric.Label,metric.Key);
    brief.Events.Add(new NewsEvent{Id="metric:"+metric.Key,Kind="指标变化",Subject=metric.Label,Domain=domain,Importance=NewsImportance.Notable,
     Facts=new List<string>{metric.Label+"由 "+Short(metric.First)+" 变为 "+Short(metric.Last)+rest(metric.Unit),
      metric.Label+"观测区间最小值 "+Short(metric.Min)+"，最大值 "+Short(metric.Max)+"，共 "+metric.Samples+" 次观测"}});
   }
   brief.Events=brief.Events.OrderByDescending(x=>x.Importance).ThenBy(x=>x.Id,StringComparer.Ordinal).ToList();
   var major=brief.MajorEvents.ToList();
   if(major.Count==0){brief.CommentaryReason="当日没有达到评论标准的重大事件";return brief;}
   var top=major[0];
   int material=reportable.Count(x=>x.Samples>1&&x.Min!=x.Max)+EditorialInput.Select(day).Count+(day.Transit?.Count??0)+breakthroughs.Count;
   // A new passenger line's verified name, stops and vehicles already give a
   // real basis for analysis; a single bare threshold does not.
   bool transitBasis=top.Kind=="线路开通"&&top.Facts.Count>=3;
   bool enough=material>=2||transitBasis;
   brief.CommentaryWarranted=enough;
   if(!enough){brief.CommentaryReason="重大事件的事实材料不足以支撑独立评论";return brief;}
   var focus=major.Take(top.Importance>=NewsImportance.Critical?2:1).SelectMany(x=>x.Facts.Take(3)).ToList();
   brief.CommentaryFocus=string.Join("；",focus);
   brief.CommentaryReason=(top.Importance>=NewsImportance.Milestone?"达到里程碑级别":"达到重大事件级别")+"，且事实材料足以展开分析";
   return brief;
  }
  static string rest(string unit)=>string.IsNullOrWhiteSpace(unit)?"":"（"+unit+"）";
  static string Truncate(string text,int max)=>string.IsNullOrEmpty(text)||text.Length<=max?text:text.Substring(0,max)+"…";
 }
}
