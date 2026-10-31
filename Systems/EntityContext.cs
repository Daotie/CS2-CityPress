using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Game.Prefabs;
using Unity.Entities;
namespace CityPress.Systems {
 internal static class EntityContext {
  internal static string Describe(object value) {
   if(value==null)return "空";
   if(value is Entity entity)return Name(entity);
   if(value is string text)return text.Length>300?text.Substring(0,300):text;
   if(value is Enum)return value.ToString();
   if(value is IFormattable formattable)return formattable.ToString(null,CultureInfo.InvariantCulture);
   if(value is IEnumerable items)return string.Join("、",items.Cast<object>().Select(Describe));
   // Only primitive parameters and known value types, never arbitrary object graphs.
   return value.GetType().IsValueType?value.ToString():"参数未展开";
  }
  internal static string Name(Entity entity) {
   if(entity==Entity.Null)return "全市";
   var system=NewsSystem.Active;if(system==null)return entity.ToString();
   var em=system.EntityManager;if(!em.Exists(entity))return entity.ToString();
   try {
    if(!em.HasComponent<Game.Vehicles.Vehicle>(entity)&&system.World.GetOrCreateSystemManaged<Game.UI.NameSystem>().TryGetCustomName(entity,out var custom)&&!string.IsNullOrWhiteSpace(custom))return custom;
    var prefabEntity=em.HasComponent<PrefabRef>(entity)?em.GetComponentData<PrefabRef>(entity).m_Prefab:entity;
    if(system.World.GetOrCreateSystemManaged<PrefabSystem>().TryGetPrefab<PrefabBase>(prefabEntity,out var prefab)&&prefab!=null) {
     var dictionary=Game.SceneFlow.GameManager.instance.localizationManager.activeDictionary;
     foreach(var prefix in new[]{"Assets.NAME[","Policies.NAME[","Services.NAME[","Statistics.NAME["})
      if(dictionary.TryGetValue(prefix+prefab.name+"]",out var label)&&!string.IsNullOrWhiteSpace(label))return label;
     return prefab.name;
    }
   } catch { }
   return entity.ToString();
  }
 }
}
