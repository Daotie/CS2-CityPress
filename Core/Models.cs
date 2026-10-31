using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace CityPress.Core {
 public sealed class Preferences {
  public bool AutoGenerate;
  public string CustomName = "";
  public string Editorial = EditorialPolicy.Default;
  public string Model = "deepseek-chat";
 }
 public sealed class ActionRecord { public string Kind, Detail; public int Count = 1; public string First, Last; }
 public sealed class MetricRecord {
  public string Key, Label, Unit, FirstTime, LastTime;
  public double First, Last, Min, Max; public int Samples;
 }
 public sealed class DayLog { public string Day; public List<ActionRecord> Actions = new List<ActionRecord>(); public List<MetricRecord> Metrics=new List<MetricRecord>(); }
 public sealed class SaveState {
  public int Version = 1;
  public string Id = Guid.NewGuid().ToString("N");
  public Preferences Override;
  public List<DayLog> Days = new List<DayLog>();
  public List<string> OmittedEmptyDays = new List<string>();
 }
 public sealed class Article { public string Section, Title, Body; }
 public sealed class Newspaper { public string Name, Day, Headline, Deck; public List<Article> Articles; public int CacheHitTokens, CacheMissTokens; public bool LocalCache, LocalOnly; }
 public static class Storage {
  public static readonly string Root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "Colossal Order", "Cities Skylines II", "ModsData", "CityPress");
  public static string Hash(string s) { using(var h=SHA256.Create()) return BitConverter.ToString(h.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-", "").ToLowerInvariant(); }
  public static void Write(string path, string text) {
   Directory.CreateDirectory(Path.GetDirectoryName(path)); var tmp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
   File.WriteAllText(tmp,text,new UTF8Encoding(false));
   if(File.Exists(path)) File.Replace(tmp,path,null); else File.Move(tmp,path);
  }
  public static T Read<T>(string path) where T:new() { return File.Exists(path) ? JsonConvert.DeserializeObject<T>(File.ReadAllText(path)) ?? new T() : new T(); }
 }
 public static class Aggregator {
  public static void Sample(DayLog log,string key,string label,string unit,double value,string time) {
   if(double.IsNaN(value)||double.IsInfinity(value))return;
   var metric=log.Metrics.FirstOrDefault(x=>x.Key==key);
   if(metric==null){metric=new MetricRecord{Key=key,Label=label,Unit=unit,First=value,Min=value,Max=value,FirstTime=time};log.Metrics.Add(metric);}
   metric.Last=value;metric.Min=Math.Min(metric.Min,value);metric.Max=Math.Max(metric.Max,value);metric.LastTime=time;metric.Samples++;
  }
  public static void Add(DayLog log, string kind, string detail, string time) {
   var old=log.Actions.FirstOrDefault(x=>x.Kind==kind && x.Detail==detail);
   if(old!=null) { old.Count++; old.Last=time; return; }
   // Preserve every distinct action on disk; bound only the later API projection.
   log.Actions.Add(new ActionRecord {Kind=kind, Detail=detail, First=time, Last=time});
  }
 }
}
