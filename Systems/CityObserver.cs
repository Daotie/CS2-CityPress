using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
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
  struct Asset {public Entity Prefab;public string Category,Settings;public int State,Slot,SeenPass;}
  Dictionary<Entity,Asset> previous=new Dictionary<Entity,Asset>();
  readonly Dictionary<Entity,string> events=new Dictionary<Entity,string>();
  readonly HashSet<Entity> seenCreated=new HashSet<Entity>(),seenDeleted=new HashSet<Entity>();
  readonly HashSet<Entity> liveCreated=new HashSet<Entity>(),liveDeleted=new HashSet<Entity>();
  readonly List<Entity> tracked=new List<Entity>();
  NativeArray<Entity> scanEntities,journalEntities;NativeArray<CityStatisticsSystem.StatisticsKey> statisticKeys;
  int scanCursor,sweepCursor,scanPass,statisticCursor,journalCursor,dataStage=-1,pollutionCursor,pollutionNonzero,pollutionLength;
  double pollutionSum,pollutionMax,maxSliceMs;bool eventsPrimed;
  DateTime nextReport;
  sealed class MetricMeta {public string Id,Label,Unit;}
  readonly Dictionary<CityStatisticsSystem.StatisticsKey,MetricMeta> metricMetadata=new Dictionary<CityStatisticsSystem.StatisticsKey,MetricMeta>();
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
   if(epoch!=news.SessionEpoch||now<lastTime){DisposeWork();epoch=news.SessionEpoch;primed=eventsPrimed=false;previous.Clear();tracked.Clear();events.Clear();seenCreated.Clear();seenDeleted.Clear();metricMetadata.Clear();scanPass=0;dataStage=-1;nextSnapshot=nextStatistics=default;assetError=statError=eventError=bindingError=cityPolicies=null;maxSliceMs=0;}
   lastTime=now;
   long started=Stopwatch.GetTimestamp();
   if(primed)Guard("assets",()=>Changes(news));
   if(dataStage<0&&now>=nextStatistics)dataStage=0;
   if(dataStage>=0){int stage=dataStage;bool ok=Guard("statistics",()=>{
     if(stage==0)Statistics(news);
     else if(stage==1){AdditionalStatistics(news);dataStage++;}
     else if(stage==2)Journal(news);
     else if(stage==3){GameplayBindings.Scan(news);dataStage++;}
     else Pollution(news,stage-4);
    });
    if(!ok){if(statisticKeys.IsCreated)statisticKeys.Dispose();if(journalEntities.IsCreated)journalEntities.Dispose();pollutionCursor=0;pollutionSum=pollutionMax=0;pollutionNonzero=0;dataStage++;}
    if(dataStage>6){dataStage=-1;nextStatistics=now.AddMinutes(5);}
   }else if(scanEntities.IsCreated||now>=nextSnapshot){if(!Guard("assets",()=>Snapshot(news))){if(scanEntities.IsCreated)scanEntities.Dispose();nextSnapshot=now.AddMinutes(5);}}
   maxSliceMs=Math.Max(maxSliceMs,(Stopwatch.GetTimestamp()-started)*1000.0/Stopwatch.Frequency);
   if(DateTime.UtcNow>=nextReport){nextReport=DateTime.UtcNow.AddSeconds(2);
    Report="城市观测："+previous.Count+" 个对象，"+metricCount+" 项统计；分帧校对，采集切片峰值 "+maxSliceMs.ToString("F1")+" ms";
    var errors=new[]{assetError,statError,eventError,bindingError}.Where(e=>e!=null).ToArray();if(errors.Length>0)Report+="；部分采集异常："+string.Join("、",errors);maxSliceMs=0;
   }
  }
  void DisposeWork(){if(scanEntities.IsCreated)scanEntities.Dispose();if(statisticKeys.IsCreated)statisticKeys.Dispose();if(journalEntities.IsCreated)journalEntities.Dispose();pollutionCursor=pollutionLength=0;pollutionSum=pollutionMax=0;pollutionNonzero=0;}
  protected override void OnDestroy(){DisposeWork();base.OnDestroy();}
  void Track(Entity entity,Asset value){if(previous.TryGetValue(entity,out var old))value.Slot=old.Slot;else{value.Slot=tracked.Count;tracked.Add(entity);}value.SeenPass=scanPass;previous[entity]=value;}
  void Untrack(Entity entity){if(!previous.TryGetValue(entity,out var value))return;int last=tracked.Count-1;var tail=tracked[last];tracked[value.Slot]=tail;if(tail!=entity){var moved=previous[tail];moved.Slot=value.Slot;previous[tail]=moved;}tracked.RemoveAt(last);previous.Remove(entity);if(value.Slot<sweepCursor)sweepCursor=value.Slot;}
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
   if(EntityManager.HasComponent<Game.Objects.Tree>(entity))return new Asset{Prefab=EntityManager.GetComponentData<PrefabRef>(entity).m_Prefab,Category="树木",Settings=""};
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
   if((old.State&2)==0&&(current.State&2)!=0)Fact(news,"出现废弃状态",current);
   if((old.State&4)==0&&(current.State&4)!=0)Fact(news,"出现损毁状态",current);
   if((old.State&8)==0&&(current.State&8)!=0)Fact(news,"出现火情",current);
  }
  void Snapshot(NewsSystem news) {
   if(!scanEntities.IsCreated){scanEntities=assets.ToEntityArray(Allocator.Persistent);scanCursor=sweepCursor=0;scanPass++;}
   var slice=new WorkSlice(384);
   while(scanCursor<scanEntities.Length&&slice.Take()){
    var entity=scanEntities[scanCursor++];
    if(!EntityManager.Exists(entity)||!EntityManager.HasComponent<PrefabRef>(entity)||EntityManager.HasComponent<Game.Common.Deleted>(entity)||EntityManager.HasComponent<Game.Common.Owner>(entity)||EntityManager.HasComponent<Game.Tools.Temp>(entity))continue;
    var value=Read(entity);
    if(primed){if(previous.TryGetValue(entity,out var old))Compare(news,old,value);else Fact(news,(value.State&1)!=0?"进入施工":"新增实体（运营状态未核实）",value);}
    Track(entity,value);
   }
   if(scanCursor<scanEntities.Length)return;
   while(sweepCursor<tracked.Count&&slice.Take()){
    var entity=tracked[sweepCursor];var old=previous[entity];
    if(old.SeenPass!=scanPass){if(primed)Fact(news,"移除",old);Untrack(entity);}else sweepCursor++;
   }
   if(sweepCursor<tracked.Count)return;
   scanEntities.Dispose();
   var city=World.GetExistingSystemManaged<CitySystem>();if(city!=null){var value=Policies(city.City);if(primed&&cityPolicies!=null&&cityPolicies!=value)news.Record("城市实况 · 全市政策变化","原："+cityPolicies+"；现："+value);cityPolicies=value;}
   primed=true;nextSnapshot=lastTime.AddMinutes(5);
  }
  string Policies(Entity entity) {
   if(!EntityManager.HasBuffer<Game.Policies.Policy>(entity))return "";
   var buffer=EntityManager.GetBuffer<Game.Policies.Policy>(entity,true);if(buffer.Length==0)return "";
   using(var values=buffer.ToNativeArray(Allocator.Temp))return string.Join("；",values.ToArray().OrderBy(p=>p.m_Policy.Index).Select(p=>EntityContext.Name(p.m_Policy)+"="+p.m_Flags+"，调整值="+p.m_Adjustment));
  }
  void Changes(NewsSystem news) {
   // Poll narrow transient queries at ModificationEnd so short lived entities do
   // not vanish between the five minute reconciliation snapshots.
   if(created.CalculateEntityCount()==0)seenCreated.Clear();else using(var entities=created.ToEntityArray(Allocator.Temp)) {
    liveCreated.Clear();foreach(var entity in entities){liveCreated.Add(entity);if(!seenCreated.Add(entity)||previous.ContainsKey(entity))continue;var asset=Read(entity);Track(entity,asset);Fact(news,(asset.State&1)!=0?"进入施工":"新增实体（运营状态未核实）",asset);}seenCreated.IntersectWith(liveCreated);
   }
   if(deleted.CalculateEntityCount()==0)seenDeleted.Clear();else using(var entities=deleted.ToEntityArray(Allocator.Temp)) {
    liveDeleted.Clear();foreach(var entity in entities){liveDeleted.Add(entity);if(!seenDeleted.Add(entity))continue;if(previous.TryGetValue(entity,out var asset)){Fact(news,"移除",asset);Untrack(entity);}}seenDeleted.IntersectWith(liveDeleted);
   }
  }
  void Statistics(NewsSystem news) {
   var stats=World.GetOrCreateSystemManaged<CityStatisticsSystem>();stats.CompleteWriters();
   var lookup=stats.GetLookup();if(!lookup.IsCreated){dataStage++;return;}
   if(!statisticKeys.IsCreated){statisticKeys=lookup.GetKeyArray(Allocator.Persistent);statisticCursor=0;metricCount=0;}
   var slice=new WorkSlice(48);
   while(statisticCursor<statisticKeys.Length&&slice.Take()) {
    var key=statisticKeys[statisticCursor++];if(!lookup.TryGetValue(key,out var entity))continue;
    if(entity==Entity.Null||!EntityManager.Exists(entity)||!EntityManager.HasBuffer<Game.City.CityStatistic>(entity))continue;
    var samples=EntityManager.GetBuffer<Game.City.CityStatistic>(entity,true);if(samples.Length==0)continue;
    var sample=samples[samples.Length-1];
    if(!metricMetadata.TryGetValue(key,out var meta)){
     meta=new MetricMeta{Id=key.type+"/"+key.parameter,Label=key.type+"（原生分组 "+key.parameter+"）",Unit="原生单位；分组含义未经核实不能当成全市总量"};
     if(EntityManager.HasComponent<PrefabRef>(entity)) {
      var prefab=EntityManager.GetComponentData<PrefabRef>(entity).m_Prefab;meta.Label=EntityContext.Name(prefab)+" / "+key.type+"（分组 "+key.parameter+"）";
      if(EntityManager.HasComponent<StatisticsData>(prefab)){var data=EntityManager.GetComponentData<StatisticsData>(prefab);meta.Unit=data.m_UnitType+"；统计口径="+data.m_CollectionType+"；分组编号不表示全市总量";}
     }
     metricMetadata[key]=meta;
    }
    news.Sample("native/"+meta.Id,meta.Label,meta.Unit,sample.m_TotalValue);
    news.Sample("native-sample/"+meta.Id,meta.Label+"（单次原生样本，非当日总计）",meta.Unit,sample.m_Value);
    metricCount++;
   }
   if(statisticCursor<statisticKeys.Length)return;statisticKeys.Dispose();dataStage++;
  }
  void AdditionalStatistics(NewsSystem news) {
   var city=World.GetOrCreateSystemManaged<CitySystem>();
   bool unlimited=EntityManager.HasComponent<Game.City.PlayerMoney>(city.City)&&EntityManager.GetComponentData<Game.City.PlayerMoney>(city.City).m_Unlimited;
   news.Sample(unlimited?"city/displayMoneyUnlimited":"city/money","市库现有资金",unlimited?"无限资金模式显示值，不得作为真实财政突破":"原生货币单位，资金存量",city.moneyAmount);
   if(EntityManager.HasComponent<Game.City.Population>(city.City))news.Sample("city/population","常住人口","人",EntityManager.GetComponentData<Game.City.Population>(city.City).m_Population);
   var budget=World.GetExistingSystemManaged<CityServiceBudgetSystem>();
   if(budget!=null){var income=budget.GetIncomeArray(out var incomeDeps);var expenses=budget.GetExpenseArray(out var expenseDeps);incomeDeps.Complete();expenseDeps.Complete();long gross=0,signedExpenses=0;foreach(var amount in income)gross+=amount;foreach(var amount in expenses)signedExpenses+=amount;
    if(gross!=0||signedExpenses!=0){news.Sample("city/monthlyIncome","月度预算总收入","原生货币单位/月，预算面板口径，非本月已结算收入",gross);news.Sample("city/monthlyExpenses","月度预算支出","原生货币单位/月，预算面板口径",Math.Abs(signedExpenses));news.Sample("city/monthlyBalance","月度预算收支结余","原生货币单位/月，预算收入加原生负支出，非市库余额",gross+signedExpenses);}
   }
   var households=World.GetExistingSystemManaged<BudgetSystem>();if(households!=null&&households.HasData)news.Sample("city/households","家庭数量","户",households.GetHouseholdCount());
   if(EntityManager.HasComponent<Game.City.Tourism>(city.City)){var tourism=EntityManager.GetComponentData<Game.City.Tourism>(city.City);news.Sample("city/tourists","当前游客人数","人，当前在城数量，非累计接待量",tourism.m_CurrentTourists);news.Sample("city/lodgingCapacity","住宿接待容量","原生住宿容量单位",tourism.m_Lodging.y);}
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
   if(!journalEntities.IsCreated){journalEntities=journals.ToEntityArray(Allocator.Persistent);journalCursor=0;}
   var slice=new WorkSlice(48);
   while(journalCursor<journalEntities.Length&&slice.Take()) {
    var entity=journalEntities[journalCursor++];if(!EntityManager.Exists(entity)||!EntityManager.HasComponent<Game.Events.EventJournalEntry>(entity))continue;
    string detail="事件档案="+entity+"；"+EntityContext.Name(system.GetPrefab(entity));
    if(system.TryGetData(entity,out var data))foreach(var item in data)detail+="；"+item.m_Type+"="+item.m_Value;
    if(system.TryGetCityEffects(entity,out var effects))foreach(var item in effects)detail+="；"+item.m_Type+"：起始="+item.m_StartValue+"，当前="+item.m_Value;
    detail+="；"+(EntityManager.HasComponent<Game.Events.EventJournalCompleted>(entity)?"事件已结束":"事件记录中");
    if(eventsPrimed&&(!events.TryGetValue(entity,out var old)||old!=detail))news.Record("城市事件（累计观测，不得重复相加）",detail);
    events[entity]=detail;
   }
   if(journalCursor<journalEntities.Length)return;journalEntities.Dispose();eventsPrimed=true;dataStage++;
  }
  void Pollution(NewsSystem news,int channel) {
   if(channel==0){var system=World.GetExistingSystemManaged<AirPollutionSystem>();if(system==null){dataStage++;return;}var map=system.GetMap(true,out var deps);deps.Complete();PollutionMap(news,"air","空气污染",map,value=>value.m_Pollution);}
   else if(channel==1){var system=World.GetExistingSystemManaged<GroundPollutionSystem>();if(system==null){dataStage++;return;}var map=system.GetMap(true,out var deps);deps.Complete();PollutionMap(news,"ground","土壤污染",map,value=>value.m_Pollution);}
   else {var system=World.GetExistingSystemManaged<NoisePollutionSystem>();if(system==null){dataStage++;return;}var map=system.GetMap(true,out var deps);deps.Complete();PollutionMap(news,"noise","噪声污染",map,value=>value.m_Pollution);}
  }
  void PollutionMap<T>(NewsSystem news,string key,string label,NativeArray<T> map,Func<T,short> read) where T:struct {
   if(!map.IsCreated||map.Length==0){dataStage++;return;}
   if(pollutionLength!=map.Length){pollutionLength=map.Length;pollutionCursor=pollutionNonzero=0;pollutionSum=pollutionMax=0;}
   var slice=new WorkSlice(2048);
   while(pollutionCursor<map.Length&&slice.Take()){var value=Math.Max(0,(int)read(map[pollutionCursor++]));pollutionSum+=value;pollutionMax=Math.Max(pollutionMax,value);if(value>0)pollutionNonzero++;}
   if(pollutionCursor<map.Length)return;
   const string unit="原生污染强度，全地图分批观测，非同一瞬间快照；不是人口暴露值、AQI、分贝或现实浓度";
   news.Sample("pollution/"+key+"/mean",label+"全地图平均强度",unit,pollutionSum/map.Length);
   news.Sample("pollution/"+key+"/max",label+"全地图最高强度",unit,pollutionMax);
   news.Sample("pollution/"+key+"/affected",label+"非零栅格占比","0—1比例，全地图分批观测，含城外未开发地区，非受影响人口占比",(double)pollutionNonzero/map.Length);
   pollutionCursor=pollutionNonzero=pollutionLength=0;pollutionSum=pollutionMax=0;dataStage++;
  }
 }
}
