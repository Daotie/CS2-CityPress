using System;
using System.Text.RegularExpressions;
namespace CityPress.Core {
 public static class CapturePolicy {
  // Only stock gameplay bindings. Never inspect mod settings, credentials or account UI.
  public static bool IsGameplayTrigger(string owner,string name) {
   if(owner==null||!owner.StartsWith("Game.UI.InGame.",StringComparison.Ordinal)||string.IsNullOrEmpty(name))return false;
   if(Regex.IsMatch(owner,"Camera|Infoview|Photo|Radio|Tutorial|Chirp|Notification|Statistics|Charts|TimeUISystem|ToolOptions|Toolbar|GameScreen|Menu|Search|AssetPack|AssetMenu|Debug",RegexOptions.IgnoreCase))return false;
   if(Regex.IsMatch(name,"^(?:on)?(?:select|deselect|focus|highlight|hover|open|close|show|hide|view|inspect|navigate|read|play|pause|scroll|zoom|sort|filter|search|expand|collapse|dismiss|subscribe|unsubscribe|request|resetLoanOffer)|infoview|snap|brush|preview|tooltip|tab$",RegexOptions.IgnoreCase))return false;
   if(Regex.IsMatch(name,"^(?:on)?set(?:selected|category|tab|panel|screen|sampleRange)|^toggle.*(?:panel|overlay|view)$",RegexOptions.IgnoreCase))return false;
   return Regex.IsMatch(name,"(?:set|change|create|delete|remove|upgrade|toggle|buy|purchase|relocate|accept|pay|rename|add|apply|bulldoze|activate|deactivate)",RegexOptions.IgnoreCase);
  }
 }
}
