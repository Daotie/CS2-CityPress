using System;
namespace CityPress.Core {
 public static class ApiOptions {
  public const string DefaultEndpoint="https://api.deepseek.com/chat/completions";
  public static Uri Endpoint(string input){
   if(string.IsNullOrWhiteSpace(input))throw new ArgumentException("请填写 API 地址。");input=input.Trim();
   if(input.Length>1000||!Uri.TryCreate(input,UriKind.Absolute,out var uri)||(uri.Scheme!="http"&&uri.Scheme!="https")||string.IsNullOrEmpty(uri.Host)||!string.IsNullOrEmpty(uri.UserInfo)||!string.IsNullOrEmpty(uri.Query)||!string.IsNullOrEmpty(uri.Fragment))throw new ArgumentException("API 地址应为 HTTP/HTTPS 地址，不含账号、查询参数或片段。");
   var builder=new UriBuilder(uri);var path=uri.AbsolutePath.TrimEnd('/');builder.Path=path;
   if(!path.EndsWith("/chat/completions",StringComparison.OrdinalIgnoreCase))builder.Path=(path.Length==0?(uri.Host.Equals("api.deepseek.com",StringComparison.OrdinalIgnoreCase)?"":"/v1"):path)+"/chat/completions";
   return builder.Uri;
  }
  public static string CredentialId(string endpoint)=>Storage.Hash(Endpoint(endpoint).AbsoluteUri);
  public static bool LegacyDeepSeek(string endpoint){var uri=Endpoint(endpoint);return uri.Scheme=="https"&&uri.Host.Equals("api.deepseek.com",StringComparison.OrdinalIgnoreCase)&&uri.IsDefaultPort&&(uri.AbsolutePath=="/chat/completions"||uri.AbsolutePath=="/v1/chat/completions");}
  public static void Validate(Preferences prefs){Endpoint(prefs.ApiEndpoint);if(prefs.MaxOutputTokens<512||prefs.MaxOutputTokens>65536||prefs.TimeoutSeconds<15||prefs.TimeoutSeconds>600||(prefs.TokenParameter!="max_tokens"&&prefs.TokenParameter!="max_completion_tokens")||(prefs.LayoutMode!="auto"&&prefs.LayoutMode!="varied"))throw new ArgumentException("API 输出上限、等待时间或版式选项无效。");}
  public static void Repair(Preferences prefs){var defaults=new Preferences();if(prefs.MaxOutputTokens<512||prefs.MaxOutputTokens>65536)prefs.MaxOutputTokens=defaults.MaxOutputTokens;if(prefs.TimeoutSeconds<15||prefs.TimeoutSeconds>600)prefs.TimeoutSeconds=defaults.TimeoutSeconds;if(prefs.TokenParameter!="max_tokens"&&prefs.TokenParameter!="max_completion_tokens")prefs.TokenParameter=defaults.TokenParameter;if(prefs.LayoutMode!="auto"&&prefs.LayoutMode!="varied")prefs.LayoutMode=defaults.LayoutMode;}
 }
}
