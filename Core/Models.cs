using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace CityPress.Core {
 public sealed class Preferences {
  public bool AutoGenerate;
  public string CustomName = "";
  public string Editorial = EditorialPolicy.Default;
  public string Model = "deepseek-chat";
  public string ExportDirectory = "";
  public string ApiEndpoint=ApiOptions.DefaultEndpoint;
  public bool RequireApiKey=true,JsonMode=true,LongCommentary=true;
  public int MaxOutputTokens=8192,TimeoutSeconds=180;
  public string TokenParameter="max_tokens",LayoutMode="auto";
  // Editing controls added in beta.7. Defaults stay on for existing settings,
  // because a missing JSON field keeps the field initializer.
  public bool QualityCheck=true,AutoRevision=true,DedicatedCommentary=true;
 }
 public sealed class ActionRecord { public string Kind, Detail; public int Count = 1; public string First, Last; }
 public sealed class MetricRecord {
  public string Key, Label, Unit, FirstTime, LastTime;
  public double First, Last, Min, Max; public int Samples;
 }
 public sealed class BreakthroughRecord { public string Key,Label,Unit,Kind,Time;public double PreviousHigh,Threshold,Value; }
 public sealed class MilestoneProgress { public double High; }
 public sealed class TransitRecord {public string Id,Name,Mode,Time,Change;public bool Enabled,Operating;public int Stops,Vehicles,Fare;}
 public sealed class DayLog { public string Day; public List<ActionRecord> Actions = new List<ActionRecord>(); public List<MetricRecord> Metrics=new List<MetricRecord>(); public List<BreakthroughRecord> Breakthroughs=new List<BreakthroughRecord>(); public List<TransitRecord> Transit=new List<TransitRecord>(); }
 public sealed class SaveState {
  public int Version = 1;
  public string Id = Guid.NewGuid().ToString("N");
  public Preferences Override;
  public List<DayLog> Days = new List<DayLog>();
  public List<string> OmittedEmptyDays = new List<string>();
  public Dictionary<string,MilestoneProgress> MilestoneProgress = new Dictionary<string,MilestoneProgress>();
 }
  // Severity is "fatal" (must not publish), "major" (targeted revision) or "minor".
  public sealed class QualityIssue { public string Code,Detail,Severity="minor";public bool Fatal=>Severity=="fatal"; }
  public sealed class Article {
   public string Section, Title, Body, Genre;
   // Debug/audit metadata: never rendered into the newspaper body.
   public string Id;
   public List<string> Sources=new List<string>();
   public List<QualityIssue> Issues=new List<QualityIssue>();
   public double Score=100;public int Revision;public string Status="pending";
   // "长评"/"短评" on the commentary edition; "" elsewhere.
   public string CommentaryRole;
  }
 public sealed class LayoutBlock {public string Kind;public int Article=-1,Start,Count=-1;}
 public sealed class LayoutColumn {public int Width;public List<LayoutBlock> Blocks=new List<LayoutBlock>();}
 public sealed class PageComposition {public string Template;public List<LayoutBlock> Top=new List<LayoutBlock>();public List<LayoutColumn> Columns=new List<LayoutColumn>();}
 public sealed class NewspaperPage {public string Theme,Headline,Deck;public List<Article> Articles=new List<Article>();public PageComposition Composition;}
 public sealed class Newspaper { public string Name, Day, Headline, Deck,Provider,Model; public List<Article> Articles; public List<NewspaperPage> Pages; public int CacheHitTokens, CacheMissTokens,InputTokens,OutputTokens;public bool CacheUsageReported,UsageReported; public bool LocalCache, LocalOnly;
  // Beta.7 editorial pipeline metadata (persisted for auditing, not shown in the paper).
  public int QualityScore=100;public string QualityNote;public List<QualityIssue> Issues=new List<QualityIssue>();
  public int RevisionCount;public string LayoutVersion;public string CommentaryFocus;public int DroppedUnsafeArticles;
 }
 public static class Storage {
  public static readonly string Root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow", "Colossal Order", "Cities Skylines II", "ModsData", "CityPress");
  public static string Hash(string s) { using(var h=SHA256.Create()) return BitConverter.ToString(h.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-", "").ToLowerInvariant(); }
  public static void Write(string path, string text) {
   Directory.CreateDirectory(Path.GetDirectoryName(path)); var tmp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
   File.WriteAllText(tmp,text,new UTF8Encoding(false));
   Commit(tmp,path);
  }
  // File.Replace is atomic but is denied by some sandboxed, network or
  // anti-virus mediated file systems. Never lose the new content for that.
  public static void Commit(string tmp,string path) {
   if(!File.Exists(path)){File.Move(tmp,path);return;}
   try{File.Replace(tmp,path,null);return;}catch(IOException){}catch(UnauthorizedAccessException){}
   File.Delete(path);File.Move(tmp,path);
  }
  public static T Read<T>(string path) where T:new() { return File.Exists(path) ? JsonConvert.DeserializeObject<T>(File.ReadAllText(path)) ?? new T() : new T(); }
 }
 public static class Aggregator {
  struct ActionKey:IEquatable<ActionKey> {public string Kind,Detail;public bool Equals(ActionKey other)=>Kind==other.Kind&&Detail==other.Detail;public override bool Equals(object other)=>other is ActionKey key&&Equals(key);public override int GetHashCode()=>unchecked((Kind?.GetHashCode()??0)*397^(Detail?.GetHashCode()??0));}
  sealed class Index {public int Actions,Metrics,Total;public bool ActionMapReady,CountReady;public readonly Dictionary<ActionKey,ActionRecord> ActionMap=new Dictionary<ActionKey,ActionRecord>();public readonly Dictionary<string,MetricRecord> MetricMap=new Dictionary<string,MetricRecord>();}
  static readonly ConditionalWeakTable<DayLog,Index> indexes=new ConditionalWeakTable<DayLog,Index>();
  static Index Lookup(DayLog log) {
   var index=indexes.GetValue(log,_=>new Index());
   if(!index.ActionMapReady||index.Actions!=log.Actions.Count){index.ActionMap.Clear();index.Total=0;foreach(var action in log.Actions){if(action==null)continue;index.ActionMap[new ActionKey{Kind=action.Kind,Detail=action.Detail}]=action;index.Total+=action.Count;}index.Actions=log.Actions.Count;index.ActionMapReady=index.CountReady=true;}
   if(index.Metrics!=log.Metrics.Count){index.MetricMap.Clear();foreach(var metric in log.Metrics)if(metric?.Key!=null)index.MetricMap[metric.Key]=metric;index.Metrics=log.Metrics.Count;}
   return index;
  }
  public static int Count(DayLog log){var index=indexes.GetValue(log,_=>new Index());if(!index.CountReady||index.Actions!=log.Actions.Count){index.Total=log.Actions.Where(x=>x!=null).Sum(x=>x.Count);index.Actions=log.Actions.Count;index.CountReady=true;index.ActionMapReady=false;}return index.Total;}
  public static void Sample(DayLog log,string key,string label,string unit,double value,string time) {
   if(double.IsNaN(value)||double.IsInfinity(value))return;
   var index=Lookup(log);index.MetricMap.TryGetValue(key,out var metric);
   if(metric==null){metric=new MetricRecord{Key=key,Label=label,Unit=unit,First=value,Min=value,Max=value,FirstTime=time};log.Metrics.Add(metric);index.MetricMap[key]=metric;index.Metrics=log.Metrics.Count;}
   metric.Last=value;metric.Min=Math.Min(metric.Min,value);metric.Max=Math.Max(metric.Max,value);metric.LastTime=time;metric.Samples++;
  }
  public static void Add(DayLog log, string kind, string detail, string time) {
   var index=Lookup(log);var key=new ActionKey{Kind=kind,Detail=detail};index.Total++;
   if(index.ActionMap.TryGetValue(key,out var old)) { old.Count++; old.Last=time; return; }
   // Preserve every distinct action on disk; bound only the later API projection.
   var action=new ActionRecord {Kind=kind, Detail=detail, First=time, Last=time};log.Actions.Add(action);index.ActionMap[key]=action;index.Actions=log.Actions.Count;
  }
 }
}
