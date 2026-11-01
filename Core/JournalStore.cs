using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
namespace CityPress.Core {
 public sealed class JournalIndex {
  public int Version=2;public string Id;public Preferences Override;
  public List<string> Days=new List<string>(),OmittedEmptyDays=new List<string>();
  public Dictionary<string,MilestoneProgress> MilestoneProgress=new Dictionary<string,MilestoneProgress>();
 }
 public sealed class JournalStore {
  readonly string root;string save;
  readonly Dictionary<string,DayLog> written=new Dictionary<string,DayLog>();
  public JournalStore(string dataRoot=null){root=dataRoot??Storage.Root;}
  string City(string id)=>Path.Combine(root,"cities",id);
  // Called by a single serialized worker. Historical days are immutable and
  // written once per session, rather than serializing years of history every tick.
  public void Write(SaveState snapshot,CancellationToken token) {
   if(save!=snapshot.Id){written.Clear();save=snapshot.Id;}
   var live=new HashSet<string>();
   foreach(var day in snapshot.Days){token.ThrowIfCancellationRequested();if(!GameCalendar.IsValidDay(day.Day))continue;live.Add(day.Day);
    if(written.TryGetValue(day.Day,out var old)&&ReferenceEquals(day,old))continue;
    Storage.Write(Path.Combine(City(save),"journals",day.Day+".json"),JsonConvert.SerializeObject(day));written[day.Day]=day;
   }
   foreach(var day in written.Keys.Where(day=>!live.Contains(day)).ToArray())written.Remove(day);
   token.ThrowIfCancellationRequested();
   var index=new JournalIndex{Id=snapshot.Id,Override=snapshot.Override,Days=snapshot.Days.Where(day=>GameCalendar.IsValidDay(day.Day)).Select(day=>day.Day).ToList(),OmittedEmptyDays=snapshot.OmittedEmptyDays,MilestoneProgress=snapshot.MilestoneProgress};
   Storage.Write(Path.Combine(City(save),"journal.index.json"),JsonConvert.SerializeObject(index));
   // Legacy journal.json is intentionally left untouched for recovery.
  }
  public SaveState Read(string id) {
   var path=Path.Combine(City(id),"journal.index.json");if(!File.Exists(path))return Storage.Read<SaveState>(Path.Combine(City(id),"journal.json"));
   var index=Storage.Read<JournalIndex>(path);if(index.Version!=2)throw new InvalidDataException("不支持的日志索引版本");
   return new SaveState{Id=id,Override=index.Override,OmittedEmptyDays=index.OmittedEmptyDays,MilestoneProgress=index.MilestoneProgress,Days=index.Days.Where(GameCalendar.IsValidDay).Select(day=>Storage.Read<DayLog>(Path.Combine(City(id),"journals",day+".json"))).ToList()};
  }
 }
}
