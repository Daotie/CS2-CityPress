using System.Collections.Generic;
using System.Linq;
namespace CityPress.Core {
 public static class JournalSnapshot {
  // Completed days are immutable. Only today's mutable rows need a deep copy;
  // JSON serialization and disk writes then run on a worker without world access.
  public static SaveState Capture(SaveState source) {
   var preferences=source.Override==null?null:new Preferences{AutoGenerate=source.Override.AutoGenerate,CustomName=source.Override.CustomName,Editorial=source.Override.Editorial,Model=source.Override.Model,ExportDirectory=source.Override.ExportDirectory};
   var result=new SaveState{Version=source.Version,Id=source.Id,Override=preferences,Days=source.Days.ToList(),OmittedEmptyDays=source.OmittedEmptyDays?.ToList()??new List<string>(),MilestoneProgress=(source.MilestoneProgress??new Dictionary<string,MilestoneProgress>()).ToDictionary(x=>x.Key,x=>new MilestoneProgress{High=x.Value.High})};
   if(result.Days.Count>0)result.Days[result.Days.Count-1]=CopyDay(result.Days[result.Days.Count-1]);
   return result;
  }
  public static DayLog CopyDay(DayLog old)=>new DayLog{Day=old.Day,Actions=old.Actions.Select(x=>new ActionRecord{Kind=x.Kind,Detail=x.Detail,Count=x.Count,First=x.First,Last=x.Last}).ToList(),Metrics=old.Metrics.Select(x=>new MetricRecord{Key=x.Key,Label=x.Label,Unit=x.Unit,First=x.First,Last=x.Last,Min=x.Min,Max=x.Max,Samples=x.Samples,FirstTime=x.FirstTime,LastTime=x.LastTime}).ToList(),Breakthroughs=(old.Breakthroughs??new List<BreakthroughRecord>()).Select(x=>new BreakthroughRecord{Key=x.Key,Label=x.Label,Unit=x.Unit,Kind=x.Kind,Time=x.Time,PreviousHigh=x.PreviousHigh,Threshold=x.Threshold,Value=x.Value}).ToList()};
 }
}
