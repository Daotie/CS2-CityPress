using System;
using System.Collections.Generic;
using System.Linq;
using CityPress.Core;
using Game;
using Game.Prefabs;
using Game.Simulation;
using Unity.Collections;
using Unity.Entities;
namespace CityPress.Systems {
 // Observation is deliberately separate from UI intent. Existing cities are baselined,
 // never reported as freshly built when a save is opened.
 public partial class CityObserver : GameSystemBase {
  internal static string Report="城市观测：等待存档加载";
  EntityQuery assets,created,deleted,journals,ownedTiles;
  struct Asset {public Entity Prefab;public string Category,Settings;public int State;}
  Dictionary<Entity,Asset> previous=new Dictionary<Entity,Asset>();
  readonly Dictionary<Entity,string> events=new Dictionary<Entity,string>();
  readonly HashSet<Entity> seenCreated=new HashSet<Entity>(),seenDeleted=new HashSet<Entity>();
  int epoch=-1;DateTime nextSnapshot,nextStatistics,lastTime;bool primed;
  string assetError,statError,eventError,bindingError,cityPolicies;int metricCount;
  protected override void OnCreate() {
   base.OnCreate();
   var roots=new[]{ComponentType.ReadOnly<Game.Buildings.Building>(),ComponentType.ReadOnly<Game.Net.Edge>(),ComponentType.ReadOnly<Game.Areas.Area>(),ComponentType.ReadOnly<Game.Routes.TransportLine>(),ComponentType.ReadOnly<Game.Objects.Tree>()};
   assets=GetEntityQuery(new EntityQueryDesc{All=new[]{ComponentType.ReadOnly<PrefabRef>()},Any=roots,None=new[]{ComponentType.ReadOnly<Game.Tools.Temp>(),ComponentType.ReadOnly<Game.Common.Owner>(),ComponentType.ReadOnly<Game.Common.Deleted>()}});
   created=GetEntityQuery(new EntityQueryDesc{All=new[]{ComponentType.ReadOnly<PrefabRef>(),ComponentType.ReadOnly<Game.Common.Created>()},Any=roots,None=new[]{ComponentType.ReadOnly<Game.Tools.Temp>(),ComponentType.ReadOnly<Game.Common.Owner>(),ComponentType.ReadOnly<Game.Common.Deleted>()}});
   deleted=GetEntityQuery(new EntityQueryDesc{All=new[]{ComponentType.ReadOnly<PrefabRef>(),ComponentType.ReadOnly<Game.Common.Deleted>()},Any=roots,None=new[]{ComponentType.ReadOnly<Game.Tools.Temp>(),ComponentType.ReadOnly<Game.Common.Owner>()}});
   journals=GetEntityQuery(ComponentType.ReadOnly<Game.Events.EventJournalEntry>());
   ownedTiles=GetEntityQuery(new EntityQueryDesc{All=new[]{ComponentType.ReadOnly<Game.Areas.MapTile>()},None=new[]{ComponentType.ReadOnly<Game.Common.Native>(),ComponentType.ReadOnly<Game.Common.Deleted>()}});
  }
  protected override void OnUpdate() {
   var news=NewsSystem.Active;if(news==null||!news.TryGetGameTime(out var now))return;
   if(epoch!=news.SessionEpoch||now<lastTime){epoch=news.SessionEpoch;primed=false;previous.Clear();events.Clear();seenCreated.Clear();seenDeleted.Clear();nextSnapshot=nextStatistics=default;assetError=statError=eventError=bindingError=cityPolicies=null;}
   lastTime=now;
   if(!primed||now>=nextSnapshot) {
    bool success=Guard("assets",()=>Snapshot(news));
    Guard("events",()=>Journal(news));
    Guard("bindings",()=>GameplayBindings.Scan(news));
    nextSnapshot=now.AddMinutes(5);primed=primed||success;
   }
   if(primed)Guard("assets",()=>Changes(news));
   if(now>=nextStatistics){Guard("statistics",()=>Statistics(news));Guard("statistics",()=>Pollution(news));nextStatistics=now.AddMinutes(5);}
   Report="城市观测："+previous.Count+" 个设施/路网/区域/线路/树木，"+metricCount+" 项统计；每 5 游戏分钟快照";
   var errors=new[]{assetError,statError,eventError,bindingError}.Where(e=>e!=null).ToArray();
   if(errors.Length>0)Report+="；部分采集异常："+string.Join("、",errors);
  }
  bool Guard(string channel,Action action) {
   try{action();return true;}catch(Exception e){string message=channel+"/"+e.GetType().Name;
    if(channel=="assets"){if(assetError==null)Mod.Log.Warn("城市采集异常："+message+"\n"+e.StackTrace);assetError=message;}
    else if(channel=="statistics"){if(statError==null)Mod.Log.Warn("城市采集异常："+message+"\n"+e.StackTrace);statError=message;}
    else if(channel=="events"){if(eventError==null)Mod.Log.Warn("城市采集异常："+message+"\n"+e.StackTrace);eventError=message;}
    else bindingError=message;
    return false;
   }
  }
  Asset Read(Entity entity) {
   int state=0;
   if(EntityManager.HasComponent<Game.Objects.UnderConstruction>(entity))state|=1;
   if(EntityManager.HasComponent<Game.Buildings.Abandoned>(entity))state|=2;
   if(EntityManager.HasComponent<Game.Common.Destroyed>(entity))state|=4;
   if(EntityManager.HasComponent<Game.Events.OnFire>(entity))state|=8;
   if(EntityManager.HasComponent<Game.Areas.MapTile>(entity)&&!EntityManager.HasComponent<Game.Common.Native>(entity))state|=16;
   string settings=Policies(entity);
   if(EntityManager.HasComponent<Game.Routes.Route>(entity)){var route=EntityManager.GetComponentData<Game.Routes.Route>(entity);settings+="；线路选项="+route.m_OptionMask;}
   if(EntityManager.HasComponent<Game.Routes.TransportLine>(entity)){var line=EntityManager.GetComponentData<Game.Routes.TransportLine>(entity);settings+="；票价="+line.m_TicketPrice;}
   if(EntityManager.HasBuffer<Game.Buildings.InstalledUpgrade>(entity))foreach(var upgrade in EntityManager.GetBuffer<Game.Buildings.InstalledUpgrade>(entity,true))settings+="；附属升级="+EntityContext.Name(upgrade.m_Upgrade)+"，选项="+upgrade.m_OptionMask;
   return new Asset{Prefab=EntityManager.GetComponentData<PrefabRef>(entity).m_Prefab,State=state,Settings=settings,
    Category=EntityManager.HasComponent<Game.Buildings.Building>(entity)?"建筑":EntityManager.HasComponent<Game.Net.Edge>(entity)?"路网区段（非整条道路）":EntityManager.HasComponent<Game.Routes.TransportLine>(entity)?"公共交通线路":EntityManager.HasComponent<Game.Objects.Tree>(entity)?"树木":"区域"};
  }
  void Fact(NewsSystem news,string action,Asset asset) {news.Record("城市实况 · "+asset.Category+action,EntityContext.Name(asset.Prefab));}
  void Compare(NewsSystem news,Asset old,Asset current) {
   if(old.Prefab!=current.Prefab)Fact(news,"类型变更",current);
   if(old.Settings!=current.Settings)news.Record("城市实况 · 设置变化",EntityContext.Name(current.Prefab)+"；原："+old.Settings+"；现："+current.Settings);
   if((old.State&16)==0&&(current.State&16)!=0)Fact(news,"用地已解锁",current);
   if((old.State&1)!=0&&(current.State&1)==0&&(current.State&4)==0)Fact(news,"施工标记结束（不等于已投入运营）",current);
   if((old.State&1)==0&&(current.State&1)!=0)Fact(news,"进入施工",current);
   if((old.State&8)!=0&&(current.State&8)==0)Fact(news,"火情标记解除",current);
   if((old.State&2)!=0&&(current.State&2)==0)Fact(news,"废弃标记解除",current);
   foreach(var state in new[]{Tuple.Create(2,"出现废弃状态"),Tuple.Create(4,"出现损毁状态"),Tuple.Create(8,"出现火情")})
    if((old.State&state.Item1)==0&&(current.State&state.Item1)!=0)Fact(news,state.Item2,current);
  }
  void Snapshot(NewsSystem news) {
   var next=new Dictionary<Entity,Asset>();
   using(var entities=assets.ToEntityArray(Allocator.Temp))foreach(var entity in entities) {
    var value=Read(entity);next[entity]=value;
    if(!primed)continue;
    if(previous.TryGetValue(entity,out var old))Compare(news,old,value);
    else Fact(news,(value.State&1)!=0?"进入施工":"新增实体（运营状态未核实）",value);
   }
   if(primed)foreach(var pair in previous)if(!next.ContainsKey(pair.Key))Fact(news,"移除",pair.Value);
   previous=next;
   var city=World.GetExistingSystemManaged<CitySystem>();if(city!=null){var value=Policies(city.City);if(primed&&cityPolicies!=null&&cityPolicies!=value)news.Record("城市实况 · 全市政策变化","原："+cityPolicies+"；现："+value);cityPolicies=value;}
  }
  string Policies(Entity entity) {
   if(!EntityManager.HasBuffer<Game.Policies.Policy>(entity))return "";
   using(var values=EntityManager.GetBuffer<Game.Policies.Policy>(entity,true).ToNativeArray(Allocator.Temp))return string.Join("；",values.ToArray().OrderBy(p=>p.m_Policy.Index).Select(p=>EntityContext.Name(p.m_Policy)+"="+p.m_Flags+"，调整值="+p.m_Adjustment));
  }
  void Changes(NewsSystem news) {
   // Poll narrow transient queries at ModificationEnd so short lived entities do
   // not vanish between the five minute reconciliation snapshots.
   if(created.CalculateEntityCount()==0)seenCreated.Clear();else using(var entities=created.ToEntityArray(Allocator.Temp)) {
    var live=new HashSet<Entity>();foreach(var entity in entities){live.Add(entity);if(!seenCreated.Add(entity)||previous.ContainsKey(entity))continue;var asset=Read(entity);previous[entity]=asset;Fact(news,(asset.State&1)!=0?"进入施工":"新增实体（运营状态未核实）",asset);}seenCreated.IntersectWith(live);
   }
   if(deleted.CalculateEntityCount()==0)seenDeleted.Clear();else using(var entities=deleted.ToEntityArray(Allocator.Temp)) {
    var live=new HashSet<Entity>();foreach(var entity in entities){live.Add(entity);if(!seenDeleted.Add(entity))continue;if(previous.TryGetValue(entity,out var asset)){Fact(news,"移除",asset);previous.Remove(entity);}}seenDeleted.IntersectWith(live);
   }
  }
  void Statistics(NewsSystem news) {
   var stats=World.GetOrCreateSystemManaged<CityStatisticsSystem>();stats.CompleteWriters();
   var lookup=stats.GetLookup();if(!lookup.IsCreated)return;
   metricCount=0;
   using(var keys=lookup.GetKeyArray(Allocator.Temp))foreach(var key in keys) {
    var entity=lookup[key];if(entity==Entity.Null||!EntityManager.HasBuffer<Game.City.CityStatistic>(entity))continue;
    var samples=EntityManager.GetBuffer<Game.City.CityStatistic>(entity,true);if(samples.Length==0)continue;
    var sample=samples[samples.Length-1];var id=key.type+"/"+key.parameter;
    string label=key.type+"（原生分组 "+key.parameter+"）",unit="原生单位；分组含义未经核实不能当成全市总量";
    if(EntityManager.HasComponent<PrefabRef>(entity)) {
     var prefab=EntityManager.GetComponentData<PrefabRef>(entity).m_Prefab;
     label=EntityContext.Name(prefab)+" / "+key.type+"（分组 "+key.parameter+"）";
     if(EntityManager.HasComponent<StatisticsData>(prefab)){var data=EntityManager.GetComponentData<StatisticsData>(prefab);unit=data.m_UnitType+"；统计口径="+data.m_CollectionType+"；分组编号不表示全市总量";}
    }
    news.Sample("native/"+id,label,unit,sample.m_TotalValue);
    news.Sample("native-sample/"+id,label+"（单次原生样本，非当日总计）",unit,sample.m_Value);
    metricCount++;
   }
   var city=World.GetOrCreateSystemManaged<CitySystem>();news.Sample("city/money","市库现有资金","货币",city.moneyAmount);
   news.Sample("city/ownedTiles","已解锁用地单元","块，不等于平方公里",ownedTiles.CalculateEntityCount());
   if(EntityManager.HasComponent<Loan>(city.City))news.Sample("city/loan","现有贷款余额","货币",EntityManager.GetComponentData<Loan>(city.City).m_Amount);
   var electricity=World.GetExistingSystemManaged<ElectricityStatisticsSystem>();if(electricity!=null) {
    news.Sample("electricity/production","供电能力","原生电力单位，不得未经换算写成兆瓦",electricity.production);
    news.Sample("electricity/consumption","用电需求","原生电力单位",electricity.consumption);
    news.Sample("electricity/fulfilled","实际供电","原生电力单位",electricity.fulfilledConsumption);
    news.Sample("electricity/batteryCharge","储能当前存量","原生储能单位",electricity.batteryCharge);
    news.Sample("electricity/batteryCapacity","储能容量","原生储能单位",electricity.batteryCapacity);
   }
   var water=World.GetExistingSystemManaged<WaterStatisticsSystem>();if(water!=null) {
    news.Sample("water/capacity","供水能力","原生供水单位，不得未经换算写成立方米",water.freshCapacity);
    news.Sample("water/consumption","供水需求","原生供水单位",water.freshConsumption);
    news.Sample("water/fulfilled","实际供水","原生供水单位",water.fulfilledFreshConsumption);
    news.Sample("sewage/capacity","污水处理能力","原生水量单位",water.sewageCapacity);
    news.Sample("sewage/consumption","污水处理需求","原生水量单位",water.sewageConsumption);
    news.Sample("sewage/fulfilled","实际污水处理","原生水量单位",water.fulfilledSewageConsumption);
   }
   var climate=World.GetExistingSystemManaged<ClimateSystem>();if(climate!=null){news.Sample("weather/temperature","气温","摄氏度",climate.temperature.value);news.Sample("weather/rain","降雨状态","1为降雨，0为无雨，不是降雨量",climate.isRaining?1:0);news.Sample("weather/snow","降雪状态","1为降雪，0为无雪，不是降雪量",climate.isSnowing?1:0);}
   var traffic=World.GetExistingSystemManaged<TrafficFlowSystem>();if(traffic!=null){news.Sample("traffic/flow","交通流畅度原始指标","原生比例，非拥堵道路条数",traffic.cityAverageTrafficFlow);news.Sample("traffic/volume","交通流量原始指标","原生单位，非日车辆总数",traffic.cityAverageTrafficVolume);}
  }
  void Journal(NewsSystem news) {
   var system=World.GetOrCreateSystemManaged<Game.Events.EventJournalSystem>();
   using(var entities=journals.ToEntityArray(Allocator.Temp))foreach(var entity in entities) {
    string detail="事件档案="+entity+"；"+EntityContext.Name(system.GetPrefab(entity));
    if(system.TryGetData(entity,out var data))foreach(var item in data)detail+="；"+item.m_Type+"="+item.m_Value;
    if(system.TryGetCityEffects(entity,out var effects))foreach(var item in effects)detail+="；"+item.m_Type+"：起始="+item.m_StartValue+"，当前="+item.m_Value;
    detail+="；"+(EntityManager.HasComponent<Game.Events.EventJournalCompleted>(entity)?"事件已结束":"事件记录中");
    if(primed&&(!events.TryGetValue(entity,out var old)||old!=detail))news.Record("城市事件（累计观测，不得重复相加）",detail);
    events[entity]=detail;
   }
  }
  void Pollution(NewsSystem news) {
   var air=World.GetExistingSystemManaged<AirPollutionSystem>();if(air!=null){var map=air.GetMap(true,out var dependencies);dependencies.Complete();PollutionMap(news,"air","空气污染",map,value=>value.m_Pollution);}
   var ground=World.GetExistingSystemManaged<GroundPollutionSystem>();if(ground!=null){var map=ground.GetMap(true,out var dependencies);dependencies.Complete();PollutionMap(news,"ground","土壤污染",map,value=>value.m_Pollution);}
   var noise=World.GetExistingSystemManaged<NoisePollutionSystem>();if(noise!=null){var map=noise.GetMap(true,out var dependencies);dependencies.Complete();PollutionMap(news,"noise","噪声污染",map,value=>value.m_Pollution);}
  }
  static void PollutionMap<T>(NewsSystem news,string key,string label,NativeArray<T> map,Func<T,short> read) where T:struct {
   if(!map.IsCreated||map.Length==0)return;
   double sum=0,max=0;int nonzero=0;foreach(var item in map){var value=Math.Max(0,(int)read(item));sum+=value;max=Math.Max(max,value);if(value>0)nonzero++;}
   const string unit="原生污染强度，全地图栅格统计，不是人口暴露值、AQI、分贝或现实污染物浓度";
   news.Sample("pollution/"+key+"/mean",label+"全地图平均强度",unit,sum/map.Length);
   news.Sample("pollution/"+key+"/max",label+"全地图最高强度",unit,max);
   news.Sample("pollution/"+key+"/affected",label+"非零栅格占比","0—1比例，范围含城外未开发地区，不是受影响人口占比",(double)nonzero/map.Length);
  }
 }
}
