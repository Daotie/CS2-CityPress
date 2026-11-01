using System.Diagnostics;
namespace CityPress.Core {
 public struct WorkSlice {
  int remaining;readonly long deadline;
  public WorkSlice(int maximum,double milliseconds=1.5){remaining=maximum;deadline=Stopwatch.GetTimestamp()+(long)(Stopwatch.Frequency*milliseconds/1000);}
  public bool Take(){if(remaining<=0||Stopwatch.GetTimestamp()>deadline)return false;remaining--;return true;}
 }
}
