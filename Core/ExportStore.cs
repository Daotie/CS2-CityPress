using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
namespace CityPress.Core {
 public sealed class ArchiveResult {public Newspaper Paper;public string Folder;public bool ExportDeferred,ExportRenamed,ExportRedirected,ArchiveDeferred;}
 public static class ExportPaths {
  public static readonly string DefaultDirectory=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"CityPress");
  public static string Root(string custom) {
   if(string.IsNullOrWhiteSpace(custom))return DefaultDirectory;
   custom=custom.Trim();
   if(custom.Length>1000||!Regex.IsMatch(custom,@"^(?:[a-zA-Z]:[\\/]|\\\\[^\\/]+[\\/][^\\/]+(?:[\\/]|$))"))throw new ArgumentException("导出位置必须是完整的文件夹路径。");
   return Path.GetFullPath(custom);
  }
  public static string Segment(string name) {
   name=string.IsNullOrWhiteSpace(name)?"城市日报":name.Trim();
   string safe=Regex.Replace(name,@"[<>:""/\\|?*\x00-\x1f]","_").TrimEnd(' ','.');
   if(string.IsNullOrWhiteSpace(safe)||safe=="."||safe=="..")safe="城市日报";
   if(Regex.IsMatch(safe,@"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)",RegexOptions.IgnoreCase))safe="_"+safe;
   if(safe.Length>72)safe=safe.Substring(0,72).TrimEnd();
   if(safe!=name)safe+="-"+Storage.Hash(name).Substring(0,8);
   return safe;
  }
  public static string Folder(string custom,string newspaper,string saveId)=>Path.Combine(Root(custom),Segment(newspaper),"存档-"+Segment(saveId));
 }
 public sealed class ExportStore {
  readonly string dataRoot;
  public ExportStore(string root=null){dataRoot=root??Storage.Root;}
  public string Issues(string saveId)=>Path.Combine(dataRoot,"cities",saveId,"issues");
  string Index(string saveId)=>Path.Combine(dataRoot,"cities",saveId,"exports.json");
  public ArchiveResult Save(string saveId,Newspaper paper,string custom,CancellationToken token) {
   if(paper==null||!GameCalendar.IsValidDay(paper.Day))throw new InvalidOperationException("报纸日期无效，未导出。");
   token.ThrowIfCancellationRequested();
   var result=new ArchiveResult{Paper=paper,Folder=Issues(saveId)};
   var json=JsonConvert.SerializeObject(paper);
   // Publish the local archive before optional Word export. A locked Word file
   // must never discard an already generated newspaper or its in-game preview.
   try{Storage.Write(Path.Combine(Issues(saveId),paper.Day+".json"),json);}
   catch(IOException){result.ArchiveDeferred=true;}catch(UnauthorizedAccessException){result.ArchiveDeferred=true;}
   token.ThrowIfCancellationRequested();
   string requested;try{requested=ExportPaths.Folder(custom,paper.Name,saveId);}catch(ArgumentException){requested=Issues(saveId);result.ExportRedirected=true;}
   var folders=new[]{requested,Issues(saveId)}.Distinct(StringComparer.OrdinalIgnoreCase);
   bool saved=false;
   foreach(var folder in folders){
    token.ThrowIfCancellationRequested();bool renamed=false;
    try{
     try{DocxExporter.Save(Path.Combine(folder,paper.Day+".docx"),paper);}
     catch(IOException){renamed=true;DocxExporter.Save(Path.Combine(folder,paper.Day+"-"+DateTime.Now.ToString("HHmmss")+"-"+Guid.NewGuid().ToString("N").Substring(0,8)+".docx"),paper);}
     result.Folder=folder;result.ExportRenamed=renamed;result.ExportRedirected|=!string.Equals(folder,requested,StringComparison.OrdinalIgnoreCase);saved=true;
     try{Storage.Write(Path.Combine(folder,paper.Day+".json"),json);}catch(IOException){}catch(UnauthorizedAccessException){}
     break;
    }catch(IOException){}catch(UnauthorizedAccessException){}
   }
   result.ExportDeferred=!saved;
   token.ThrowIfCancellationRequested();
   try{
    var index=Storage.Read<Dictionary<string,string>>(Index(saveId));index[paper.Day]=result.Folder;
    Storage.Write(Index(saveId),JsonConvert.SerializeObject(index));
   }catch(IOException){}catch(UnauthorizedAccessException){}catch(JsonException){}
   return result;
  }

  public ArchiveResult Read(string saveId,string day,string custom) {
   if(!GameCalendar.IsValidDay(day))throw new InvalidDataException("无效报纸日期");
   var path=Path.Combine(Issues(saveId),day+".json");if(!File.Exists(path))throw new FileNotFoundException("报纸归档不存在");
   var paper=Storage.Read<Newspaper>(path);var index=Storage.Read<Dictionary<string,string>>(Index(saveId));
   string folder;if(!index.TryGetValue(day,out folder))folder=File.Exists(Path.Combine(Issues(saveId),day+".docx"))?Issues(saveId):ExportPaths.Folder(custom,paper.Name,saveId);
   return new ArchiveResult{Paper=paper,Folder=folder};
  }
  public string[] List(string saveId)=>Directory.Exists(Issues(saveId))?Directory.GetFiles(Issues(saveId),"*.json").Select(Path.GetFileNameWithoutExtension).Where(GameCalendar.IsValidDay).OrderByDescending(x=>x).ToArray():Array.Empty<string>();
 }
}
