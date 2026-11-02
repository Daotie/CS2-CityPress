using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using CityPress.Core;
using Colossal.UI.Binding;
using Game.UI;
namespace CityPress.Systems {
 internal static class GameplayBindings {
  sealed class Wrapped {public object Binding;public FieldInfo Field;public Delegate Original,Wrapper;}
  static readonly List<Wrapped> wrapped=new List<Wrapped>();
  static readonly HashSet<object> seen=new HashSet<object>();
  internal static int Count=>wrapped.Count;
  internal static int Failed;
  [ThreadStatic] internal static bool InPlayerAction;
  [ThreadStatic] internal static bool Detailed;
  internal static void Scan(NewsSystem news) {
   var field=typeof(UISystemBase).GetField("m_Bindings",BindingFlags.Instance|BindingFlags.NonPublic);
   if(field==null)throw new MissingFieldException("Gameplay bindings unavailable");
   foreach(var candidate in news.World.Systems) {
    if(!(candidate is UISystemBase system))continue;
    var owner=system.GetType().FullName;
    if(owner==null||!owner.StartsWith("Game.UI.InGame.",StringComparison.Ordinal))continue;
    if(!(field.GetValue(system) is IEnumerable bindings))continue;
    foreach(var binding in bindings) {
     if(binding==null||!seen.Add(binding))continue;
     try {Wrap(binding,owner);}catch{Failed++;}
    }
   }
  }
  internal static void Wrap(object binding,string owner) {
      var type=binding.GetType();var name=type.GetProperty("name")?.GetValue(binding) as string;
      if(!CapturePolicy.IsGameplayTrigger(owner,name))return;
      var callback=type.GetField("m_Callback",BindingFlags.Instance|BindingFlags.NonPublic);
      if(callback==null||!(callback.GetValue(binding) is Delegate original))return;
      // Raw readers cannot be consumed twice: keep the action and subject only.
      var parameters=original.GetType().GetMethod("Invoke").GetParameters().Select(p=>Expression.Parameter(p.ParameterType,p.Name)).ToArray();
      var args=Expression.NewArrayInit(typeof(object),parameters.Select(p=>Expression.Convert(p,typeof(object))));
      var body=Expression.Call(typeof(GameplayBindings).GetMethod(nameof(Invoke),BindingFlags.Static|BindingFlags.NonPublic),Expression.Constant(original),Expression.Constant(owner),Expression.Constant(name),args);
      var wrapper=Expression.Lambda(original.GetType(),body,parameters).Compile();
      callback.SetValue(binding,wrapper);wrapped.Add(new Wrapped{Binding=binding,Field=callback,Original=original,Wrapper=wrapper});
  }
  static void Invoke(Delegate original,string owner,string name,object[] args) {
   bool previous=InPlayerAction,previousDetailed=Detailed;InPlayerAction=true;Detailed=false;
   string before="";try{before=Describe(args);}catch{}
   try {
    try{original.DynamicInvoke(args);}catch(TargetInvocationException e){ExceptionDispatchInfo.Capture(e.InnerException??e).Throw();throw;}
    if(!Detailed)try{NewsSystem.Active?.Record("玩家操作 · "+owner.Substring("Game.UI.InGame.".Length)+"."+name,before.Length==0?"操作已提交，具体结果以城市观测为准":before);}catch{}
   }finally{InPlayerAction=previous;Detailed=previousDetailed||Detailed;}
  }
  static string Describe(object[] args) {
   var text=string.Join(" | ",args.Where(a=>!(a is IJsonReader)).Select(EntityContext.Describe));
   var selected=NewsSystem.Active?.World.GetExistingSystemManaged<Game.UI.InGame.SelectedInfoUISystem>();
   if(selected!=null&&selected.selectedEntity!=Unity.Entities.Entity.Null)text+="；选中对象："+EntityContext.Name(selected.selectedEntity);
   return text;
  }
  internal static void Dispose(){foreach(var item in wrapped)try{if(Equals(item.Field.GetValue(item.Binding),item.Wrapper))item.Field.SetValue(item.Binding,item.Original);}catch{}wrapped.Clear();seen.Clear();Failed=0;}
 }
}
