using System;
using System.Collections.Generic;
using System.Linq;
namespace CityPress.Core {
 public static class Milestones {
  sealed class Rule {public string Label,Unit;public bool Population;public double Minimum;}
  static readonly Dictionary<string,Rule> rules=new Dictionary<string,Rule>{
   {"city/population",new Rule{Label="常住人口",Unit="人",Population=true,Minimum=1000}},
   {"city/money",new Rule{Label="市库资金余额",Unit="原生货币单位，资金存量",Minimum=100000}},
   {"city/monthlyIncome",new Rule{Label="月度预算总收入",Unit="原生货币单位/月，预算面板口径，非本月已结算收入",Minimum=100000}},
   {"city/monthlyBalance",new Rule{Label="月度预算收支结余",Unit="原生货币单位/月，预算口径，非市库余额",Minimum=100000}},
   {"city/households",new Rule{Label="家庭数量",Unit="户",Minimum=1000}},
   {"city/tourists",new Rule{Label="当前游客人数",Unit="人，当前在城数量，非累计接待量",Minimum=1000}},
   {"city/ownedTiles",new Rule{Label="已解锁用地",Unit="用地单元，不等于平方公里",Minimum=10}}
  };
  static double Step(Rule rule,double value) {
   if(rule.Population)return value<10000?1000:value<100000?10000:value<500000?50000:value<1000000?100000:Math.Max(500000,Math.Pow(10,Math.Floor(Math.Log10(Math.Max(1,value))))/2);
   return Math.Max(rule.Minimum,Math.Pow(10,Math.Floor(Math.Log10(Math.Max(1,value))))/10);
  }
  public static bool Observe(SaveState state,DayLog day,string key,double value,string time) {
   if(double.IsNaN(value)||double.IsInfinity(value)||!rules.TryGetValue(key,out var rule))return false;
   if(state.MilestoneProgress==null)state.MilestoneProgress=new Dictionary<string,MilestoneProgress>();
   if(!state.MilestoneProgress.TryGetValue(key,out var progress)){
    // A zero finance value while budgets initialize is not a reliable baseline.
    if(key.StartsWith("city/monthly",StringComparison.Ordinal)&&value==0)return false;
    state.MilestoneProgress[key]=new MilestoneProgress{High=value};return false;
   }
   double before=progress.High;if(value<=before)return false;progress.High=value;
   string kind="数量级突破";double threshold;
   if(key=="city/monthlyBalance"&&before<=0&&value>0){threshold=0;kind="预算由亏转盈";}
   else {double step=Step(rule,Math.Max(0,before));double next=(Math.Floor(Math.Max(0,before)/step)+1)*step;if(value<next)return false;threshold=Math.Floor(value/Step(rule,value))*Step(rule,value);}
   if(day.Breakthroughs==null)day.Breakthroughs=new List<BreakthroughRecord>();
   var record=day.Breakthroughs.FirstOrDefault(x=>x.Key==key&&x.Kind==kind);
   if(record==null){record=new BreakthroughRecord{Key=key,Label=rule.Label,Unit=rule.Unit,Kind=kind,PreviousHigh=before};day.Breakthroughs.Add(record);}
   record.Threshold=Math.Max(record.Threshold,threshold);record.Value=value;record.Time=time;return true;
  }
 }
}
