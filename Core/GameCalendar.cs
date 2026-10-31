using System;
using System.Globalization;

namespace CityPress.Core {
 public static class GameCalendar {
  // TimeSystem returns a default year-one value before city time is restored.
  public static bool IsReady(DateTime time)=>time.Year>1;
  public static bool IsValidDay(string day)=>DateTime.TryParseExact(day,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var date)&&IsReady(date);
  public static int RemoveInvalidDays(SaveState state)=>state.Days.RemoveAll(day=>day==null||!IsValidDay(day.Day));
 }
}
