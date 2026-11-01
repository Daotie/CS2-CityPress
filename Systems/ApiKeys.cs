using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CityPress.Core;
namespace CityPress.Systems {
 internal static class ApiKeys {
  static string PathFor(string endpoint)=>Path.Combine(Storage.Root,"credentials",ApiOptions.CredentialId(endpoint)+".dpapi");
  internal static bool Exists(string endpoint){try{return File.Exists(PathFor(endpoint))||(ApiOptions.LegacyDeepSeek(endpoint)&&(File.Exists(Path.Combine(Storage.Root,"key.dpapi"))||!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY"))));}catch(ArgumentException){return false;}}
  internal static void Save(string endpoint,string key){var path=PathFor(endpoint);Directory.CreateDirectory(Path.GetDirectoryName(path));var data=ProtectedData.Protect(Encoding.UTF8.GetBytes(key.Trim()),null,DataProtectionScope.CurrentUser);var tmp=path+"."+Guid.NewGuid().ToString("N")+".tmp";try{File.WriteAllBytes(tmp,data);if(File.Exists(path))File.Replace(tmp,path,null);else File.Move(tmp,path);}finally{if(File.Exists(tmp))File.Delete(tmp);}}
  internal static string Read(string endpoint){
   try{
   var path=PathFor(endpoint);if(File.Exists(path))return Encoding.UTF8.GetString(ProtectedData.Unprotect(File.ReadAllBytes(path),null,DataProtectionScope.CurrentUser));
   if(!ApiOptions.LegacyDeepSeek(endpoint))return "";
   path=Path.Combine(Storage.Root,"key.dpapi");if(File.Exists(path))return Encoding.UTF8.GetString(ProtectedData.Unprotect(File.ReadAllBytes(path),null,DataProtectionScope.CurrentUser));
   return Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY")??"";
   }catch(CryptographicException){return "";}catch(IOException){return "";}catch(UnauthorizedAccessException){return "";}
  }
 }
}
