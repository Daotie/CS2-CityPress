using System;
using System.Linq;

namespace CityPress.Core {
 // UI strings are translated by the game's localization dictionary in the UI.
 // Core code only needs the active locale for language-aware content handling and
 // for localizing status keys that the UI cannot know about.
 public static class I18n {
  public static string ActiveLocale="zh-HANS";
  public static bool Chinese=>ActiveLocale.StartsWith("zh",StringComparison.OrdinalIgnoreCase);
  public static string Say(string chinese,string english)=>Chinese?chinese:english;
  public static bool MostlyChinese(string text){
   if(string.IsNullOrWhiteSpace(text))return false;
   int chinese=0,latin=0;
   foreach(var c in text){
    if(c>=0x4E00&&c<=0x9FFF)chinese++;
    else if((c>='A'&&c<='Z')||(c>='a'&&c<='z'))latin++;
   }
   if(chinese==0)return false;
   return latin==0||chinese*5>=latin;
  }
 }
 // A user-facing failure with a stable translation key. The fallback keeps the
 // existing Chinese diagnostic for tests and for callers that do not translate.
 public sealed class LocalizedException:InvalidOperationException {
  public string Key{get;}
  public object[] Args{get;}
  public LocalizedException(string key,string fallback,params object[] args):base(string.Format(fallback,args??Array.Empty<object>())){Key=key;Args=args??Array.Empty<object>();}
 }
}
