import React, {useEffect, useState} from 'react';
import {ModRegistrar} from 'cs2/modding';
import {bindValue, trigger, useValue} from 'cs2/api';
import styles from './news.module.scss';
import newspaperIcon from './icons/newspaper.svg';
import {groupArticles} from './paper-layout';

interface Preferences {AutoGenerate:boolean;CustomName:string;Editorial:string;Model:string}
interface Paper {Name:string;Day:string;Headline:string;Deck:string;Articles:{Section:string;Title:string;Body:string}[];LocalCache:boolean;LocalOnly?:boolean;CacheHitTokens:number;CacheMissTokens:number}
interface State {city:string;day:string;gameTime:string;timeReady:boolean;preferences:Preferences;scope:string;busy:boolean;status:string;popup:number;paper:Paper|null;days:{Day:string;count:number}[];issues:string[];coverage:string[];keyConfigured:boolean}
const state$=bindValue<string>('CityPress','state','{}');
const send=(action:string,...args:unknown[])=>trigger('CityPress',action,...args);
const issueDate=(day:string)=>{
 const date=new Date(day+'T12:00:00Z');
 return day.replace(/^(\d{4})-(\d{2})-(\d{2})$/,'$1年$2月$3日')+(isNaN(date.getTime())?'':' 星期'+'日一二三四五六'[date.getUTCDay()]);
};
function News(){
 const raw=useValue(state$);let state:Partial<State>={};try{state=JSON.parse(raw)}catch{}
 const [open,setOpen]=useState(false),[tab,setTab]=useState('paper'),[draft,setDraft]=useState<Preferences|null>(null),[scope,setScope]=useState('save'),[key,setKey]=useState(''),[showCoverage,setShowCoverage]=useState(false);
 useEffect(()=>{if(state.popup){setOpen(true);setTab('paper')}},[state.popup]);
 useEffect(()=>{const close=(e:KeyboardEvent)=>{if(e.key==='Escape')setOpen(false)};window.addEventListener('keydown',close);return()=>window.removeEventListener('keydown',close)},[]);
 if(!state.preferences)return null;const p=state.paper;const prefs={...state.preferences,...draft};
 const edit=(field:keyof Preferences,value:string|boolean)=>setDraft({...prefs,[field]:value});
 return <><button type="button" className={styles.launcher} title="CityPress · 城市日报" aria-label="打开城市日报" onClick={()=>setOpen(!open)}><img src={newspaperIcon} alt=""/></button>{open&&<section className={styles.panel} aria-label="CityPress · 城市日报">
 <header className={styles.header}><span className={styles.badge}><img src={newspaperIcon} alt=""/></span><div className={styles.brand}><strong>CityPress</strong><small>城市日报 · {state.city}</small></div><div className={styles.clock} role="timer" aria-label="当前游戏时间"><small>当前游戏时间</small><strong>{state.gameTime||'正在同步…'}</strong></div><button className={styles.close} title="关闭" aria-label="关闭城市日报" onClick={()=>setOpen(false)}>×</button></header>
 <nav className={styles.tabs}>{[['paper','今日报纸'],['archive','往期档案'],['settings','编辑部设置']].map(([id,label])=><button key={id} type="button" className={tab===id?styles.active:''} onClick={()=>setTab(id)}>{label}</button>)}</nav>
 <div className={styles.toolbar}><span role="status">{state.status}</span><button disabled={state.busy||!state.timeReady} onClick={()=>send('generate')}>{state.busy?'正在编印…':'生成本日新闻'}</button></div>
 <main className={styles.content}>
 {tab==='paper'&&(p?<article className={styles.paper}><div className={styles.kicker}><span>城市新闻</span><span>DAILY NEWS</span></div><div className={styles.masthead}><div className={styles.edition}><strong>综合</strong><span>城市纪事</span></div><h1>{p.Name}</h1><div className={styles.pressMark}><strong>日报</strong><span>电子刊</span></div></div><div className={styles.dateline}><span>{issueDate(p.Day)}</span><span>综合版 · 01</span></div><h2 className={styles.headline}>{p.Headline}</h2><p className={styles.deck}>{p.Deck}</p><div className={styles.columns}>{groupArticles(p.Articles??[]).map((group,column)=><div className={styles.column} key={column}>{group.map((a,i)=><section key={i} className={styles.article}><div className={styles.section}>{a.Section}</div><h3>{a.Title}</h3>{(a.Body??'').split('\n').filter(Boolean).map((line,n)=><p key={n}>{line}</p>)}</section>)}</div>)}</div><footer><span>{p.Name}</span><span>综合 · 城市新闻</span></footer></article>:<div className={styles.empty}><img src={newspaperIcon} alt=""/><h2>城市的故事，从今天开始</h2><p>操作已在后台记录。点击“生成本日新闻”，为这一天留下一份报纸。</p><small>默认手动生成 · 可在编辑部设置开启每日自动发行</small></div>)}
 {tab==='paper'&&p&&<p className={styles.muted}>{p.LocalOnly?'本期暂无可刊发事件 · 未调用 API':p.LocalCache?'本地成稿缓存命中 · 未调用 API':`DeepSeek 前缀缓存命中 ${p.CacheHitTokens} tokens · 未命中 ${p.CacheMissTokens} tokens`} · 已保存 JSON 与 Word{!p.LocalOnly&&' · AI 根据城市操作记录编辑'}</p>}
 {tab==='archive'&&<div className={styles.archive}><h2>城市档案馆</h2><p>选择已发行报纸阅读，也可补编已记录日期。</p>{state.issues?.map(day=><button key={day} onClick={()=>{send('openIssue',day);setTab('paper')}}><strong>{day}</strong><span>阅读报纸 →</span></button>)}<h3>操作记录</h3>{state.days?.slice().reverse().map(day=><button key={day.Day} disabled={state.busy||!state.timeReady} onClick={()=>{send('generateDay',day.Day);setTab('paper')}}>{day.Day} · {day.count} 次记录<span>编印 →</span></button>)}</div>}
 {tab==='settings'&&<div className={styles.settings}><h2>编辑部设置</h2><div className={styles.field}><span>应用范围</span><div className={styles.scope} role="group" aria-label="应用范围">{[['save','当前存档'],['global','全局默认'],['inherit','恢复继承']].map(([id,label])=><button type="button" key={id} aria-pressed={scope===id} className={scope===id?styles.active:''} onClick={()=>setScope(id)}>{label}</button>)}</div></div><p className={styles.muted}>当前使用：{state.scope==='save'?'存档独立设置':'全局默认'}。全局默认不覆盖其他存档的独立设置。</p>
 <button type="button" className={styles.check} role="switch" aria-checked={prefs.AutoGenerate} onClick={()=>edit('AutoGenerate',!prefs.AutoGenerate)}><span className={prefs.AutoGenerate?styles.switchOn:styles.switchOff}><span/></span><span>每日自动发行并在游戏内弹出</span></button>
 <label>报纸名称<input maxLength={60} value={prefs.CustomName??''} placeholder={`${state.city}日报（留空跟随城市名）`} onChange={e=>edit('CustomName',e.target.value)}/></label>
 <label>DeepSeek API Key<input type="password" autoComplete="off" value={key} placeholder={state.keyConfigured?'密钥已配置，留空保留':'输入密钥后保存'} onChange={e=>setKey(e.target.value)}/></label><p className={styles.muted}>密钥使用 Windows 账户加密保存在本机，不写入城市存档。生成时将发送操作汇总到 DeepSeek。</p>
 <label>模型<input value={prefs.Model??''} onChange={e=>edit('Model',e.target.value)}/></label>
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
