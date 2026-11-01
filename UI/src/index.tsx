import React, {useEffect, useState} from 'react';
import {ModRegistrar} from 'cs2/modding';
import {bindValue, trigger, useValue} from 'cs2/api';
import styles from './news.module.scss';
import newspaperIcon from './icons/newspaper.svg';
import {editionPages,compositionFor,blockText,verticalColumns,PageComposition,LayoutBlock} from './paper-layout';

interface Preferences {AutoGenerate:boolean;CustomName:string;Editorial:string;Model:string;ExportDirectory?:string;ApiEndpoint?:string;RequireApiKey?:boolean;JsonMode?:boolean;MaxOutputTokens?:number;TimeoutSeconds?:number;TokenParameter?:string;LayoutMode?:string;LongCommentary?:boolean}
interface Article {Section:string;Title:string;Body:string;Genre?:string}
interface EditionPage {Theme:string;Headline:string;Deck:string;Articles:Article[];Composition?:PageComposition}
interface Paper {Name:string;Day:string;Headline:string;Deck:string;Articles:Article[];Pages?:EditionPage[];LocalCache:boolean;LocalOnly?:boolean;CacheHitTokens:number;CacheMissTokens:number;Provider?:string;Model?:string;CacheUsageReported?:boolean;UsageReported?:boolean;InputTokens?:number;OutputTokens?:number}
interface State {city:string;day:string;gameTime:string;timeReady:boolean;preferences:Preferences;scope:string;busy:boolean;status:string;popup:number;paper:Paper|null;days:{Day:string;count:number;metrics?:number;breakthroughs?:number;transit?:number}[];issues:string[];coverage:string[];keyConfigured:boolean;exportDefault:string;exportFolder:string}
const state$=bindValue<string>('CityPress','state','{}');
const clock$=bindValue<string>('CityPress','clock','');
const send=(action:string,...args:unknown[])=>trigger('CityPress',action,...args);
const issueDate=(day:string)=>{
 const date=new Date(day+'T12:00:00Z');
 return day.replace(/^(\d{4})-(\d{2})-(\d{2})$/,'$1年$2月$3日')+(isNaN(date.getTime())?'':' 星期'+'日一二三四五六'[date.getUTCDay()]);
};
function GameClock({fallback,ready}:{fallback?:string;ready?:boolean}){
 const live=useValue(clock$);const value=live==='!'?'':/^\d{4}-\d{2}-\d{2} /.test(live)?live:(ready?fallback:'');
 return <div className={styles.clock} role="timer" aria-label="当前游戏时间"><small>当前游戏时间</small><strong>{value||'正在同步…'}</strong></div>;
}
function PageBody({page}:{page:EditionPage}){
 const plan=compositionFor(page),total=plan.Columns.reduce((sum,column)=>sum+column.Width,0);
 const block=(item:LayoutBlock,key:string)=>{const text=blockText(page,item);if(!text)return null;
  if(item.Kind==='vertical-headline')return <h2 key={key} className={styles.verticalHeading} aria-label={text}>{verticalColumns(text).map((column,i)=><span aria-hidden="true" className={styles.glyphColumn} key={i}>{column.map((char,n)=><span key={n}>{char}</span>)}</span>)}</h2>;
  if(item.Kind==='headline')return <h2 key={key} className={styles.headline}>{text}</h2>;
  if(item.Kind==='deck')return <p key={key} className={styles.deck}>{text}</p>;
  if(item.Kind==='section')return <div key={key} className={styles.section}>{text}</div>;
  if(item.Kind==='title')return <h3 key={key}>{text}</h3>;
  return <React.Fragment key={key}>{text.split('\n').filter(Boolean).map((line,n)=><p key={n} data-body="true">{line}</p>)}</React.Fragment>;
 };
 return <div className={`${styles.composition} ${plan.Template==='digest'?styles.digestPlan:''}`} data-layout={plan.Template}>{plan.Top.map((item,i)=>block(item,'top'+i))}<div className={styles.composedColumns}>{plan.Columns.map((column,i)=><div key={i} className={styles.composedColumn} style={{width:(100*column.Width/total)+'%'}}>{column.Blocks.map((item,n)=>block(item,i+'-'+n))}</div>)}</div></div>;
}
function PaperReader({paper:p}:{paper:Paper}){
 const pages=editionPages(p);const [index,setIndex]=useState(0);const selected=Math.min(index,pages.length-1),page=pages[selected];const paperRef=React.useRef<HTMLElement>(null);
 useEffect(()=>setIndex(0),[p.Day,p.Name]);
 useEffect(()=>{if(paperRef.current?.parentElement)paperRef.current.parentElement.scrollTop=0},[selected]);
 return <><div className={styles.pageNav} role="group" aria-label="报纸版面">{pages.map((edition,i)=><button type="button" key={i} aria-pressed={selected===i} className={selected===i?styles.active:''} onClick={()=>setIndex(i)}>{`${String(i+1).padStart(2,'0')} ${edition.Theme}`}</button>)}</div><article ref={paperRef} className={styles.paper}><div className={styles.kicker}><span>城市新闻</span><span>DAILY NEWS</span></div><div className={styles.masthead}><div className={styles.edition}><strong>{page.Theme}</strong><span>城市纪事</span></div><h1>{p.Name}</h1><div className={styles.pressMark}><strong>日报</strong><span>电子刊</span></div></div><div className={styles.dateline}><span>{issueDate(p.Day)}</span><span>{`${page.Theme}版 · ${String(selected+1).padStart(2,'0')}`} </span></div><PageBody page={page}/><footer><span>{p.Name}</span><span>{`${page.Theme} · ${String(selected+1).padStart(2,'0')}`} </span></footer></article><div className={styles.pageTurn}><button disabled={selected===0} onClick={()=>setIndex(selected-1)}>上一版</button><span>{`${selected+1} / ${pages.length}`}</span><button disabled={selected===pages.length-1} onClick={()=>setIndex(selected+1)}>下一版</button></div></>;
}
function News(){
 const raw=useValue(state$);let state:Partial<State>={};try{state=JSON.parse(raw)}catch{}
 const [open,setOpen]=useState(false),[tab,setTab]=useState('paper'),[draft,setDraft]=useState<Preferences|null>(null),[scope,setScope]=useState('save'),[key,setKey]=useState(''),[showCoverage,setShowCoverage]=useState(false),[advanced,setAdvanced]=useState(false);
 useEffect(()=>{if(state.popup){setOpen(true);setTab('paper')}},[state.popup]);
 useEffect(()=>{const close=(e:KeyboardEvent)=>{if(e.key==='Escape')setOpen(false)};window.addEventListener('keydown',close);return()=>window.removeEventListener('keydown',close)},[]);
 if(!state.preferences)return null;const p=state.paper;const prefs={...state.preferences,...draft};
 const edit=(field:keyof Preferences,value:string|boolean|number)=>{if(field==='ApiEndpoint')setKey('');setDraft({...prefs,[field]:value});};
 return <><button type="button" className={styles.launcher} title="CityPress · 城市日报" aria-label="打开城市日报" onClick={()=>setOpen(!open)}><img src={newspaperIcon} alt=""/></button>{open&&<section className={styles.panel} aria-label="CityPress · 城市日报">
 <header className={styles.header}><span className={styles.badge}><img src={newspaperIcon} alt=""/></span><div className={styles.brand}><strong>CityPress</strong><small>{`城市日报 · ${state.city??''}`}</small></div><GameClock fallback={state.gameTime} ready={state.timeReady}/><button className={styles.close} title="关闭" aria-label="关闭城市日报" onClick={()=>setOpen(false)}>×</button></header>
 <nav className={styles.tabs}>{[['paper','今日报纸'],['archive','往期档案'],['settings','编辑部设置']].map(([id,label])=><button key={id} type="button" className={tab===id?styles.active:''} onClick={()=>{setTab(id);if(id==='archive')send('refreshArchive')}}>{label}</button>)}</nav>
 <div className={styles.toolbar}><span role="status">{state.status}</span><div className={styles.toolbarActions}>{tab==='paper'&&p&&<><button disabled={state.busy} onClick={()=>send('exportIssue')}>导出当前报纸</button><button onClick={()=>send('openExportFolder')}>打开导出文件夹</button></>}<button disabled={state.busy||!state.timeReady} onClick={()=>send('generate')}>{state.busy?'正在处理…':'生成本日新闻'}</button></div></div>
 <main className={styles.content}>
 {tab==='paper'&&(p?<PaperReader paper={p}/>:<div className={styles.empty}><img src={newspaperIcon} alt=""/><h2>城市的故事，从今天开始</h2><p>操作已在后台记录。点击“生成本日新闻”，为这一天留下一份报纸。</p><small>默认手动生成 · 可在编辑部设置开启每日自动发行</small></div>)}
 {tab==='paper'&&p&&<p className={styles.muted}>{p.LocalOnly?'本期暂无可刊发事件 · 未调用 API':p.LocalCache?'本地成稿缓存命中 · 未调用 API':`${p.Provider??'DeepSeek'} · ${p.CacheUsageReported||!p.Provider?`缓存命中 ${p.CacheHitTokens} tokens · 未命中 ${p.CacheMissTokens} tokens`:p.UsageReported?`输入 ${p.InputTokens??0} / 输出 ${p.OutputTokens??0} tokens`:'服务未返回用量'}`} · 已保存 JSON 与 Word{!p.LocalOnly&&' · AI 根据城市操作记录编辑'}</p>}
 {tab==='archive'&&<div className={styles.archive}><h2>城市档案馆</h2><p>选择已发行报纸阅读，也可补编已记录日期。</p>{state.issues?.map(day=><button key={day} onClick={()=>{send('openIssue',day);setTab('paper')}}><strong className={styles.archiveSummary}>{day}</strong><span className={styles.archiveAction}>阅读报纸 →</span></button>)}<h3>操作记录</h3>{state.days?.filter(day=>day.Day===state.day||day.count>0||(day.metrics??0)>0||(day.breakthroughs??0)>0||(day.transit??0)>0).slice().reverse().map(day=><button key={day.Day} disabled={state.busy||!state.timeReady} onClick={()=>{send('generateDay',day.Day);setTab('paper')}}><span className={styles.archiveSummary}>{`${day.Day} · ${day.count} 次记录${day.metrics?` · ${day.metrics} 项指标`:''}${day.breakthroughs?` · ${day.breakthroughs} 项重大突破`:''}${day.transit?` · ${day.transit} 条线路`:''}`}</span><span className={styles.archiveAction}>编印 →</span></button>)}</div>}
 {tab==='settings'&&<div className={styles.settings}><h2>编辑部设置</h2><div className={styles.field}><span>应用范围</span><div className={styles.scope} role="group" aria-label="应用范围">{[['save','当前存档'],['global','全局默认'],['inherit','恢复继承']].map(([id,label])=><button type="button" key={id} aria-pressed={scope===id} className={scope===id?styles.active:''} onClick={()=>{setScope(id);if(id==='inherit')setKey('')}}>{label}</button>)}</div></div><p className={styles.muted}>当前使用：{state.scope==='save'?'存档独立设置':'全局默认'}。全局默认不覆盖其他存档的独立设置。</p>
 <button type="button" className={styles.check} role="switch" aria-checked={prefs.AutoGenerate} onClick={()=>edit('AutoGenerate',!prefs.AutoGenerate)}><span className={prefs.AutoGenerate?styles.switchOn:styles.switchOff}><span/></span><span>每日自动发行并在游戏内弹出</span></button>
 <label>报纸名称<input maxLength={60} value={prefs.CustomName??''} placeholder={`${state.city}日报（留空跟随城市名）`} onChange={e=>edit('CustomName',e.target.value)}/></label>
 <label>报纸导出目录<input maxLength={1000} value={prefs.ExportDirectory??''} placeholder={state.exportDefault??'文档\\CityPress'} onChange={e=>edit('ExportDirectory',e.target.value)}/></label><p className={styles.muted}>留空使用文档中的 CityPress 文件夹，再按报纸名称与存档分类。保存后生效；旧文件保留在原位置，可点击“导出当前报纸”另存到新目录，不调用 API。</p>
 <button type="button" className={styles.primary} onClick={()=>send('openExportFolder')}>打开导出文件夹</button><p className={styles.exportPath}>{state.exportFolder}</p>
 <h3>新闻生成服务</h3><p className={styles.muted}>支持 Chat Completions 兼容接口。保存地址只保存配置，点击生成时才发送城市素材。</p>
 <label>API 地址<input maxLength={1000} value={prefs.ApiEndpoint??'https://api.deepseek.com/chat/completions'} placeholder="https://服务地址/v1" onChange={e=>edit('ApiEndpoint',e.target.value)}/></label><p className={styles.muted}>可填写基础地址或完整 chat/completions 地址；本地服务也可使用 HTTP。</p>
 <label>模型<input value={prefs.Model??''} placeholder="服务提供的模型名称" onChange={e=>edit('Model',e.target.value)}/></label>
 <button type="button" className={styles.check} role="switch" aria-checked={prefs.RequireApiKey!==false} onClick={()=>edit('RequireApiKey',prefs.RequireApiKey===false)}><span className={prefs.RequireApiKey!==false?styles.switchOn:styles.switchOff}><span/></span><span>使用 API 密钥鉴权</span></button>
 <label>API 密钥<input type="password" autoComplete="off" value={key} placeholder={state.keyConfigured&&prefs.ApiEndpoint===state.preferences?.ApiEndpoint?'当前已保存服务的密钥已配置，留空保留':'为此 API 地址填写密钥'} onChange={e=>setKey(e.target.value)}/></label><p className={styles.muted}>每个服务地址分别加密保存密钥，不写入存档。切换地址不会带走原服务密钥；无需鉴权的本地服务可关闭鉴权。</p>
 <button type="button" className={styles.disclosure} aria-expanded={advanced} onClick={()=>setAdvanced(!advanced)}>接口兼容选项 {advanced?'−':'+'}</button>{advanced&&<>
 <button type="button" className={styles.check} role="switch" aria-checked={prefs.JsonMode!==false} onClick={()=>edit('JsonMode',prefs.JsonMode===false)}><span className={prefs.JsonMode!==false?styles.switchOn:styles.switchOff}><span/></span><span>发送 JSON 模式参数</span></button><p className={styles.muted}>服务不支持 response_format 时关闭；仍要求模型返回完整 JSON 报纸。</p>
 <label>最大输出 tokens<input type="number" min={512} max={65536} value={prefs.MaxOutputTokens??8192} onChange={e=>edit('MaxOutputTokens',Number(e.target.value))}/></label>
 <label>最长等待秒数<input type="number" min={15} max={600} value={prefs.TimeoutSeconds??180} onChange={e=>edit('TimeoutSeconds',Number(e.target.value))}/></label>
 <div className={styles.field}><span>输出上限参数</span><div className={styles.scope}>{['max_tokens','max_completion_tokens'].map(value=><button type="button" key={value} aria-pressed={(prefs.TokenParameter??'max_tokens')===value} className={(prefs.TokenParameter??'max_tokens')===value?styles.active:''} onClick={()=>edit('TokenParameter',value)}>{value}</button>)}</div></div></>}
 <h3>评论与版式</h3><button type="button" className={styles.check} role="switch" aria-checked={prefs.LongCommentary!==false} onClick={()=>edit('LongCommentary',prefs.LongCommentary===false)}><span className={prefs.LongCommentary!==false?styles.switchOn:styles.switchOff}><span/></span><span>重大事件安排独立长评论版</span></button>
 <div className={styles.field}><span>版式选择</span><div className={styles.scope}>{[['auto','按内容选版'],['varied','适度随机']].map(([value,label])=><button type="button" key={value} aria-pressed={(prefs.LayoutMode??'auto')===value} className={(prefs.LayoutMode??'auto')===value?styles.active:''} onClick={()=>edit('LayoutMode',value)}>{label}</button>)}</div></div><p className={styles.muted}>自动匹配通栏、侧栏主稿、纵向头条、短讯与长评论版式；随机模式只从合适的模板中选择。成稿固定版式，阅读和导出不重新抽取。</p>
 <label>编辑方针<textarea rows={5} maxLength={6000} value={prefs.Editorial??''} onChange={e=>edit('Editorial',e.target.value)}/></label><p className={styles.muted}>栏目、篇数、标题和取舍由编辑根据事实决定。固定部分只约束输出格式和事实准确性。</p>
 <button className={styles.primary} onClick={()=>{send('configure',JSON.stringify({scope,preferences:prefs,apiKey:key}));setKey('');setDraft(null)}}>保存设置</button>
 <button type="button" className={styles.disclosure} aria-expanded={showCoverage} onClick={()=>setShowCoverage(!showCoverage)}>操作采集兼容情况 {showCoverage?'−':'+'}</button>{showCoverage&&<div className={styles.coverage}><p>采集操作提交、城市实况、重要事件与统计趋势。重复内容合并，完整汇总保留；排除自动车辆状态、上下客、界面导航。采集到的素材由编辑部择要刊发。其他模组的独立行为及短于快照间隔的状态不保证覆盖。</p>{state.coverage?.map((x,i)=><div key={i}>{x}</div>)}</div>}</div>}
 </main></section>}</>;
}
// Keep a failure in this mod below the game's root React error boundary.
class NewsBoundary extends React.Component<{children:React.ReactNode},{failed:boolean}> {
 state={failed:false};
 static getDerivedStateFromError(){return {failed:true};}
 componentDidCatch(error:Error){console.error('[CityPress.UI] panel failed',error);}
 render(){return this.state.failed?<button type="button" className={styles.recovery} onClick={()=>this.setState({failed:false})}>CityPress 界面恢复</button>:this.props.children;}
}
const register:ModRegistrar=registry=>registry.append('GameTopLeft',()=> <NewsBoundary><News/></NewsBoundary>);
export default register;
