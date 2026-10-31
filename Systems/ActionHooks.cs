using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Game.Tools;
using HarmonyLib;

namespace CityPress.Systems {
 internal static class ActionHooks {
  static Harmony harmony;
  public static readonly List<string> Coverage=new List<string>();
  public static void Install() {
   harmony=new Harmony("Daotie.CityPress");
   Patch(typeof(ToolOutputSystem),"OnUpdate",nameof(ToolCommit),true);
   var targets=new Dictionary<string,string[]> {
    {"Game.Simulation.TaxSystem",new[]{"SetTaxRate","SetResidentialTaxRate","SetCommercialTaxRate","SetIndustrialTaxRate","SetOfficeTaxRate"}},
    {"Game.UI.InGame.ServiceBudgetUISystem",new[]{"SetServiceBudget","SetServiceFee"}},
    {"Game.UI.InGame.PoliciesUISystem",new[]{"SetPolicy","SetCityPolicy"}},
    {"Game.Tools.LoanSystem",new[]{"ChangeLoan"}}
   };
   foreach(var pair in targets) foreach(var name in pair.Value) Patch(typeof(ToolSystem).Assembly.GetType(pair.Key),name,nameof(Changed),false);
  }
  static void Patch(Type type,string name,string hook,bool prefix) {
   var methods=type?.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(m=>m.Name==name).ToArray()??Array.Empty<MethodInfo>();
   foreach(var method in methods) try { var hm=new HarmonyMethod(typeof(ActionHooks),hook); harmony.Patch(method,prefix:prefix?hm:null,postfix:prefix?null:hm);Coverage.Add(type.Name+"."+name); } catch {Mod.Log.Warn("操作采集挂钩不可用："+type.Name+"."+name);}
   if(methods.Length==0) Coverage.Add("未支持："+type?.Name+"."+name);
  }
  static void ToolCommit(ToolOutputSystem __instance) {
   try {
    var tools=__instance.World.GetExistingSystemManaged<ToolSystem>();var tool=tools?.activeTool;
    if(tool==null||tool.applyMode!=ApplyMode.Apply) return;
    // Observe actual apply pipeline, not mouse clicks or tool previews.
    var prefab=tool.GetPrefab();
    var purpose=prefab?.name;
    if(purpose==null&&System.Text.RegularExpressions.Regex.IsMatch(tool.toolID??"","Bulldoze|Terrain|Water",System.Text.RegularExpressions.RegexOptions.IgnoreCase))purpose="工具操作（不表示工程量，结果以城市观测为准）";
    NewsSystem.Active?.Record("工具提交",tool.toolID+" / "+(purpose??"无预制体"));
   } catch { /* A recorder must never interrupt a game action. */ }
  }
  static void Changed(MethodBase __originalMethod,object[] __args) {
   try {
    var parameters=__originalMethod.GetParameters();
    string detail=string.Join(" | ",__args.Select((x,i)=>parameters[i].Name+"="+EntityContext.Describe(x)));
    if(__originalMethod.Name=="ChangeLoan")detail="申请调整后的贷款总额（不是新增借款额，尚待执行）："+detail;
    // UI callbacks establish player provenance. Direct setters may also be used by mods.
    string source=GameplayBindings.InPlayerAction?"玩家操作 · ":"管理变更（调用来源未确认） · ";
    NewsSystem.Active?.Record(source+__originalMethod.DeclaringType.Name+"."+__originalMethod.Name,detail);
    if(GameplayBindings.InPlayerAction)GameplayBindings.Detailed=true;
   } catch { }
  }
  public static void Dispose(){GameplayBindings.Dispose();harmony?.UnpatchAll("Daotie.CityPress");Coverage.Clear();}
 }
}
