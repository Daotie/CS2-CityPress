using Game;
using Game.Modding;
using Colossal.Logging;
using CityPress.Systems;
using CityPress.Core;
using System;
using System.IO;
using Game.SceneFlow;
namespace CityPress {
 public sealed class Mod : IMod {
  public static readonly ILog Log=LogManager.GetLogger("CityPress").SetShowsErrorsInUI(false);
  public void OnLoad(UpdateSystem update) {
   Log.Info("CityPress 0.2.0-beta.2 loaded (gameplay bindings, world observations, city statistics)");
   try {
    if(GameManager.instance.modManager.TryGetExecutableAsset(this,out var asset)&&!string.IsNullOrEmpty(asset.path))
     DocxExporter.TemplatePath=Path.Combine(Path.GetDirectoryName(asset.path),"Templates","newspaper.docx");
   }catch(Exception error){Log.Warn("外部报纸模板定位失败，使用内嵌模板："+error.GetType().Name);}
   update.UpdateAt<NewsSystem>(SystemUpdatePhase.UIUpdate);
   update.UpdateAt<NewsSystem>(SystemUpdatePhase.Serialize);
   update.UpdateAt<NewsSystem>(SystemUpdatePhase.Deserialize);
   update.UpdateAt<CityObserver>(SystemUpdatePhase.ModificationEnd);
   ActionHooks.Install();
  }
  public void OnDispose(){ActionHooks.Dispose();}
 }
}
