using System;
using System.Globalization;
using System.Linq;

namespace CityPress.Core {
 public static class GameCalendar {
  // TimeSystem returns a default year-one value before city time is restored.
  public static bool IsReady(DateTime time)=>time.Year>1;
  public static bool IsValidDay(string day)=>DateTime.TryParseExact(day,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var date)&&IsReady(date);
  public static int RemoveInvalidDays(SaveState state)=>state.Days.RemoveAll(day=>day==null||!IsValidDay(day.Day));
  public static bool HasObservations(DayLog day)=>day!=null&&((day.Actions?.Any(a=>a!=null&&a.Count>0)??false)||(day.Metrics?.Any(m=>m!=null&&m.Samples>0)??false)||(day.Breakthroughs?.Count>0)||(day.Transit?.Count>0));
  // Earlier releases created an empty default-date entry before the save clock
  // was restored. Remove only unobserved past days, never real early-year saves.
  public static int PruneEmptyHistory(SaveState state,string currentDay) {
   if(!IsValidDay(currentDay))return 0;
   var empty=state.Days.Where(day=>day!=null&&IsValidDay(day.Day)&&string.CompareOrdinal(day.Day,currentDay)<0&&!HasObservations(day)).Select(day=>day.Day).Distinct().ToArray();
   if(state.OmittedEmptyDays==null)state.OmittedEmptyDays=new System.Collections.Generic.List<string>();
   foreach(var day in empty)if(!state.OmittedEmptyDays.Contains(day))state.OmittedEmptyDays.Add(day);
   return state.Days.RemoveAll(day=>day!=null&&empty.Contains(day.Day));
  }
  public static bool IsVisibleIssue(SaveState state,string day,string currentDay)=>IsValidDay(day)&&(day==currentDay||!(state.OmittedEmptyDays?.Contains(day)??false));
 }
}
