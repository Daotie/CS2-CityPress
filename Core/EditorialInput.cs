using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CityPress.Core {
 public static class EditorialInput {
  // General naming hooks include automatic vehicle labels from other mods.
  // Judge their origin, not words such as “boarding”, which may also occur
  // in a meaningful report about a new terminal or accessibility work.
  public static bool IsCandidate(string kind,string detail) {
   if(string.IsNullOrWhiteSpace(kind)||string.IsNullOrWhiteSpace(detail))return false;
   if(kind=="NameSystem.SetCustomName")return false;
   if(kind=="工具提交"&&(detail.EndsWith(" / 无预制体",StringComparison.Ordinal)||detail.EndsWith(" / null",StringComparison.Ordinal)))return false;
   if(kind.EndsWith("PoliciesUISystem.SetPolicy",StringComparison.Ordinal)||kind.EndsWith("PoliciesUISystem.SetCityPolicy",StringComparison.Ordinal)) {
    var subject=Regex.Replace(detail,@"Entity\(\d+:\d+\)|Entity\.Null|\b(?:true|false|null|target|policy|active|adjustment)\b|[\d\s|,.:()=+\-#]+","",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);
    if(string.IsNullOrWhiteSpace(subject))return false;
   }
   return true;
  }
  public static List<ActionRecord> Select(DayLog day)=>day.Actions.Where(a=>a!=null&&a.Count>0&&IsCandidate(a.Kind,a.Detail)).OrderBy(a=>a.Kind,StringComparer.Ordinal).ThenBy(a=>a.Detail,StringComparer.Ordinal).ToList();
 }
}
