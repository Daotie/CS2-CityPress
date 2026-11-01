using System.Collections.Generic;
using System.Linq;
namespace CityPress.Core {
 public static class TransitFacts {
  // A timetable or assigned vehicle alone does not prove passenger service.
  public static bool IsOperating(bool enabled,int stops,int servingVehicles)=>enabled&&stops>=2&&servingVehicles>0;
  public static string Transition(TransitRecord previous,TransitRecord current,bool baseline,bool alreadyKnownService=false) {
   if(baseline)return "";
   if(current.Operating&&(previous==null||!previous.Operating)){if(previous!=null&&!previous.Enabled)return "恢复运营";if(!alreadyKnownService)return "投入运营";}
   if(previous!=null&&previous.Enabled&&!current.Enabled)return "暂停运营";
   if(previous!=null&&previous.Name!=current.Name)return "线路更名";
   if(previous!=null&&previous.Fare!=current.Fare)return "票价调整";
   if(previous!=null&&previous.Stops!=current.Stops)return "停站调整";
   return "";
  }
  public static void Store(DayLog day,TransitRecord value) {
   if(day.Transit==null)day.Transit=new List<TransitRecord>();
   var old=day.Transit.FirstOrDefault(x=>x.Id==value.Id);
   // Preserve substantive transitions through subsequent ordinary samples.
   if(old!=null)value.Change=string.Join("；",new[]{old.Change,value.Change}.Where(x=>!string.IsNullOrEmpty(x)).SelectMany(x=>x.Split('；')).Distinct());
   if(old!=null)day.Transit.Remove(old);day.Transit.Add(value);
  }
  public static TransitRecord Copy(TransitRecord x)=>new TransitRecord{Id=x.Id,Name=x.Name,Mode=x.Mode,Time=x.Time,Change=x.Change,Enabled=x.Enabled,Operating=x.Operating,Stops=x.Stops,Vehicles=x.Vehicles,Fare=x.Fare};
 }
}
