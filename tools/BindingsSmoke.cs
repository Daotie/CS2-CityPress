using System;
using System.IO;
using System.Reflection;
using Colossal.UI.Binding;
class BindingsSmoke {
 static string managed;
 static int Main(string[] args){managed=args[0];AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{var file=Path.Combine(managed,new AssemblyName(e.Name).Name+".dll");return File.Exists(file)?Assembly.LoadFrom(file):null;};try{Run(args[1]);return 0;}catch(Exception e){for(var current=e;current!=null;current=current.InnerException){Console.WriteLine(current.GetType().FullName);try{Console.WriteLine(current.Message);}catch{}}return 1;}}
 static void Check(bool ok,string text){if(!ok)throw new Exception(text);Console.WriteLine("PASS "+text);}
 static T Binding<T>(string name,Delegate callback) where T:BindingBase {
  // Bypass the native Cohtml/logger constructor outside Unity. The actual game's
  // binding fields, callback delegate types and collector are still exercised.
  var value=(T)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(T));
  typeof(BindingBase).GetField("<name>k__BackingField",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(value,name);
  typeof(T).GetField("m_Callback",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(value,callback);return value;
 }
 static void Run(string mod) {
  Console.WriteLine("Loading gameplay collector");var assembly=Assembly.LoadFrom(mod);var type=assembly.GetType("CityPress.Systems.GameplayBindings",true);
  var wrap=type.GetMethod("Wrap",BindingFlags.NonPublic|BindingFlags.Static);var dispose=type.GetMethod("Dispose",BindingFlags.NonPublic|BindingFlags.Static);
  int calls=0,value=0;var binding=Binding<TriggerBinding<int>>("setLineState",new Action<int>(n=>{calls++;value=n;}));
  var field=binding.GetType().GetField("m_Callback",BindingFlags.NonPublic|BindingFlags.Instance);var original=field.GetValue(binding);
  wrap.Invoke(null,new object[]{binding,"Game.UI.InGame.TransportationOverviewUISystem"});
  Check(!ReferenceEquals(field.GetValue(binding),original),"实际游戏 TriggerBinding 回调成功包装");
  ((Action<int>)field.GetValue(binding))(42);Check(calls==1&&value==42,"玩家动作原样执行一次并保留参数");
  bool threw=false;var failure=Binding<TriggerBinding>("deleteLine",new Action(()=>{throw new InvalidOperationException("original failure");}));
  wrap.Invoke(null,new object[]{failure,"Game.UI.InGame.TransportationOverviewUISystem"});
  try{((Action)failure.GetType().GetField("m_Callback",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(failure))();}catch(InvalidOperationException e){threw=e.Message=="original failure";}
  Check(threw,"原操作异常类型保持不变");
  int rawCalls=0;var raw=Binding<RawTriggerBinding>("setRoute",new Action<IJsonReader>(reader=>rawCalls++));
  wrap.Invoke(null,new object[]{raw,"Game.UI.InGame.TransportationOverviewUISystem"});
  ((Action<IJsonReader>)raw.GetType().GetField("m_Callback",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(raw))(null);
  Check(rawCalls==1,"原始读取器回调直接传递，不消费参数");
  dispose.Invoke(null,null);Check(ReferenceEquals(field.GetValue(binding),original),"卸载恢复原始回调");
 }
}
