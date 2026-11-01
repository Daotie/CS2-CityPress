import React, {useEffect, useState} from 'react';
import {ModRegistrar} from 'cs2/modding';
import {bindValue, trigger, useValue} from 'cs2/api';
import styles from './news.module.scss';
import newspaperIcon from './icons/newspaper.svg';
import {editionPages,compositionFor,blockText,verticalColumns,isSubhead,subheadText,PageComposition,LayoutBlock} from './paper-layout';
import {useI18n} from './i18n';

interface Preferences {AutoGenerate:boolean;CustomName:string;Editorial:string;Model:string;ExportDirectory?:string;ApiEndpoint?:string;RequireApiKey?:boolean;JsonMode?:boolean;MaxOutputTokens?:number;TimeoutSeconds?:number;TokenParameter?:string;LayoutMode?:string;LongCommentary?:boolean}
interface Article {Section:string;Title:string;Body:string;Genre?:string}
interface EditionPage {Theme:string;Headline:string;Deck:string;Articles:Article[];Composition?:PageComposition}
interface Paper {Name:string;Day:string;Headline:string;Deck:string;Articles:Article[];Pages?:EditionPage[];LocalCache:boolean;LocalOnly?:boolean;CacheHitTokens:number;CacheMissTokens:number;Provider?:string;Model?:string;CacheUsageReported?:boolean;UsageReported?:boolean;InputTokens?:number;OutputTokens?:number}
interface StatusMessage {key:string;args?:Array<string|number>}
interface State {city:string;day:string;gameTime:string;timeReady:boolean;preferences:Preferences;scope:string;busy:boolean;status:StatusMessage|string;popup:number;paper:Paper|null;days:{Day:string;count:number;metrics?:number;breakthroughs?:number;transit?:number}[];issues:string[];coverage:string[];keyConfigured:boolean;exportDefault:string;exportFolder:string}
const state$=bindValue<string>('CityPress','state','{}');
const clock$=bindValue<string>('CityPress','clock','');
const send=(action:string,...args:unknown[])=>trigger('CityPress',action,...args);
const pad=(value:number)=>String(value).padStart(2,'0');

function GameClock({fallback,ready}:{fallback?:string;ready?:boolean}){
 const {t}=useI18n();const live=useValue(clock$);const value=live==='!'?'':/^\d{4}-\d{2}-\d{2} /.test(live)?live:(ready?fallback:'');
 return <div className={styles.clock} role="timer" aria-label={t('Clock.Label','')}><small>{t('Clock.Label','')}</small><strong>{value||t('Clock.Syncing','')}</strong></div>;
}
function PageBody({page}:{page:EditionPage}){
 const plan=compositionFor(page),total=plan.Columns.reduce((sum,column)=>sum+column.Width,0);
 const block=(item:LayoutBlock,key:string)=>{const text=blockText(page,item);if(!text)return null;
  if(item.Kind==='vertical-headline')return <h2 key={key} className={styles.verticalHeading} aria-label={text}>{verticalColumns(text).map((column,i)=><span aria-hidden="true" className={styles.glyphColumn} key={i}>{column.map((char,n)=><span key={n}>{char}</span>)}</span>)}</h2>;
  if(item.Kind==='headline')return <h2 key={key} className={styles.headline}>{text}</h2>;
  if(item.Kind==='deck')return <p key={key} className={styles.deck}>{text}</p>;
  if(item.Kind==='section')return <div key={key} className={styles.section}>{text}</div>;
  if(item.Kind==='title')return <h3 key={key}>{text}</h3>;
  if(item.Kind==='subhead')return <h4 key={key} className={styles.subhead}>{text}</h4>;
  return <React.Fragment key={key}>{text.split('\n').filter(Boolean).map((line,n)=>isSubhead(line)?<h4 key={n} className={styles.subhead}>{subheadText(line)}</h4>:<p key={n} data-body="true">{line}</p>)}</React.Fragment>;
 };
 return <div className={`${styles.composition} ${plan.Template==='digest'?styles.digestPlan:''}`} data-layout={plan.Template}>{plan.Top.map((item,i)=>block(item,'top'+i))}<div className={styles.composedColumns}>{plan.Columns.map((column,i)=><div key={i} className={styles.composedColumn} style={{width:(100*column.Width/total)+'%'}}>{column.Blocks.map((item,n)=>block(item,i+'-'+n))}</div>)}</div></div>;
}
function PaperReader({paper:p}:{paper:Paper}){
 const {t,themeLabel,date}=useI18n();const pages=editionPages(p);const [index,setIndex]=useState(0);const selected=Math.min(index,pages.length-1),page=pages[selected];const paperRef=React.useRef<HTMLElement>(null);
 useEffect(()=>setIndex(0),[p.Day,p.Name]);
 useEffect(()=>{if(paperRef.current?.parentElement)paperRef.current.parentElement.scrollTop=0},[selected]);
 return <><div className={styles.pageNav} role="group" aria-label={t('PageNav.Aria','')}>{pages.map((edition,i)=><button type="button" key={i} aria-pressed={selected===i} className={selected===i?styles.active:''} onClick={()=>setIndex(i)}>{`${pad(i+1)} ${themeLabel(edition.Theme)}`}</button>)}</div><article ref={paperRef} className={styles.paper}><div className={styles.kicker}><span>{t('Kicker.CityNews','')}</span><span>{t('Kicker.DailyNews','')}</span></div><div className={styles.masthead}><div className={styles.edition}><strong>{themeLabel(page.Theme)}</strong><span>{t('Masthead.Subtitle','')}</span></div><h1>{p.Name}</h1><div className={styles.pressMark}><strong>{t('Masthead.Daily','')}</strong><span>{t('Masthead.Electronic','')}</span></div></div><div className={styles.dateline}><span>{date(p.Day)}</span><span>{t('Dateline.Page','',themeLabel(page.Theme),pad(selected+1))}</span></div><PageBody page={page}/><footer><span>{p.Name}</span><span>{t('Footer.Page','',themeLabel(page.Theme),pad(selected+1))}</span></footer></article><div className={styles.pageTurn}><button disabled={selected===0} onClick={()=>setIndex(selected-1)}>{t('Page.Previous','')}</button><span>{t('Page.Counter','',selected+1,pages.length)}</span><button disabled={selected===pages.length-1} onClick={()=>setIndex(selected+1)}>{t('Page.Next','')}</button></div></>;
}
function News(){
 const {t,tr}=useI18n();
 const raw=useValue(state$);let state:Partial<State>={};try{state=JSON.parse(raw)}catch{}
 const [open,setOpen]=useState(false),[tab,setTab]=useState('paper'),[draft,setDraft]=useState<Preferences|null>(null),[scope,setScope]=useState('save'),[key,setKey]=useState(''),[showCoverage,setShowCoverage]=useState(false),[advanced,setAdvanced]=useState(false);
 useEffect(()=>{if(state.popup){setOpen(true);setTab('paper')}},[state.popup]);
 useEffect(()=>{const close=(e:KeyboardEvent)=>{if(e.key==='Escape')setOpen(false)};window.addEventListener('keydown',close);return()=>window.removeEventListener('keydown',close)},[]);
 if(!state.preferences)return null;const p=state.paper;const prefs={...state.preferences,...draft};
 const edit=(field:keyof Preferences,value:string|boolean|number)=>{if(field==='ApiEndpoint')setKey('');setDraft({...prefs,[field]:value});};
 const statusText=typeof state.status==='string'?state.status:state.status?tr(state.status.key,...(state.status.args??[])):'';
 const wheelScroll=(event:React.WheelEvent<HTMLDivElement>)=>{const scroller=event.currentTarget.closest('[data-scroll="content"]') as HTMLElement|null;if(scroller){event.preventDefault();scroller.scrollTop+=event.deltaY;}};
 const tabs:[string,string][]=[['paper',t('Tabs.Paper','')],['archive',t('Tabs.Archive','')],['settings',t('Tabs.Settings','')]];
 const scopes:[string,string][]=[['save',t('Settings.Scope.Save','')],['global',t('Settings.Scope.Global','')],['inherit',t('Settings.Scope.Inherit','')]];
 const layouts:[string,string][]=[['auto',t('Settings.Layout.Auto','')],['varied',t('Settings.Layout.Varied','')]];
 const usage=p?(!p.Provider||p.CacheUsageReported?t('Paper.Usage.Cache','',p.CacheHitTokens,p.CacheMissTokens):p.UsageReported?t('Paper.Usage.Tokens','',p.InputTokens??0,p.OutputTokens??0):t('Paper.Usage.Unavailable','')):'';
 const provider=p?.Provider??'DeepSeek';
 return <><button type="button" className={styles.launcher} title={t('App.Title','')} aria-label={t('App.Open','')} onClick={()=>setOpen(!open)}><img src={newspaperIcon} alt=""/></button>{open&&<section className={styles.panel} aria-label={t('App.Title','')}>
 <header className={styles.header}><span className={styles.badge}><img src={newspaperIcon} alt=""/></span><div className={styles.brand}><strong>CityPress</strong><small>{t('App.Brand','',state.city??'')}</small></div><GameClock fallback={state.gameTime} ready={state.timeReady}/><button className={styles.close} title={t('App.Close','')} aria-label={t('App.Close','')} onClick={()=>setOpen(false)}>×</button></header>
 <nav className={styles.tabs}>{tabs.map(([id,label])=><button key={id} type="button" className={tab===id?styles.active:''} onClick={()=>{setTab(id);if(id==='archive')send('refreshArchive')}}>{label}</button>)}</nav>
 <div className={styles.toolbar}><span role="status">{statusText}</span><div className={styles.toolbarActions}>{tab==='paper'&&p&&<><button disabled={state.busy} onClick={()=>send('exportIssue')}>{t('Toolbar.Export','')}</button><button onClick={()=>send('openExportFolder')}>{t('Toolbar.OpenFolder','')}</button></>}<button disabled={state.busy||!state.timeReady} onClick={()=>send('generate')}>{state.busy?t('Toolbar.Generating',''):t('Toolbar.Generate','')}</button></div></div>
 <main className={styles.content} data-scroll="content">
 {tab==='paper'&&(p?<PaperReader paper={p}/>:<div className={styles.empty}><img src={newspaperIcon} alt=""/><h2>{t('Empty.Title','')}</h2><p>{t('Empty.Body','')}</p><small>{t('Empty.Hint','')}</small></div>)}
 {tab==='paper'&&p&&<p className={styles.muted}>{(p.LocalOnly?t('Paper.NoEvents',''):p.LocalCache?t('Paper.CacheHit',''):`${provider} · ${usage}`)+' · '+t('Paper.Saved','')+(!p.LocalOnly?' · '+t('Paper.EditedByAI',''):'')}</p>}
 {tab==='archive'&&<div className={styles.archive}><h2>{t('Archive.Title','')}</h2><p>{t('Archive.Intro','')}</p>{state.issues?.map(day=><button key={day} onClick={()=>{send('openIssue',day);setTab('paper')}}><strong className={styles.archiveSummary}>{day}</strong><span className={styles.archiveAction}>{t('Archive.Read','')}</span></button>)}<h3>{t('Archive.Activity','')}</h3>{state.days?.filter(day=>day.Day===state.day||day.count>0||(day.metrics??0)>0||(day.breakthroughs??0)>0||(day.transit??0)>0).slice().reverse().map(day=><button key={day.Day} disabled={state.busy||!state.timeReady} onClick={()=>{send('generateDay',day.Day);setTab('paper')}}><span className={styles.archiveSummary}>{t('Archive.Records','',day.Day,day.count)+(day.metrics?t('Archive.Metrics','',day.metrics):'')+(day.breakthroughs?t('Archive.Breakthroughs','',day.breakthroughs):'')+(day.transit?t('Archive.Transit','',day.transit):'')}</span><span className={styles.archiveAction}>{t('Archive.Compile','')}</span></button>)}</div>}
 {tab==='settings'&&<div className={styles.settings} onWheelCapture={wheelScroll}><h2>{t('Settings.Title','')}</h2><div className={styles.field}><span>{t('Settings.Scope.Label','')}</span><div className={styles.scope} role="group" aria-label={t('Settings.Scope.Label','')}>{scopes.map(([id,label])=><button type="button" key={id} aria-pressed={scope===id} className={scope===id?styles.active:''} onClick={()=>{setScope(id);if(id==='inherit')setKey('')}}>{label}</button>)}</div></div><p className={styles.muted}>{state.scope==='save'?t('Settings.Scope.CurrentSave',''):t('Settings.Scope.CurrentGlobal','')}</p>
 <button type="button" className={styles.check} role="switch" aria-checked={prefs.AutoGenerate} onClick={()=>edit('AutoGenerate',!prefs.AutoGenerate)}><span className={prefs.AutoGenerate?styles.switchOn:styles.switchOff}><span/></span><span>{t('Settings.AutoGenerate','')}</span></button>
 <label>{t('Settings.Name','')}<input maxLength={60} value={prefs.CustomName??''} placeholder={t('Settings.Name.Placeholder','',state.city??'')} onChange={e=>edit('CustomName',e.target.value)}/></label>
 <label>{t('Settings.ExportDir','')}<input maxLength={1000} value={prefs.ExportDirectory??''} placeholder={state.exportDefault??t('Settings.ExportDir.Placeholder','')} onChange={e=>edit('ExportDirectory',e.target.value)}/></label><p className={styles.muted}>{t('Settings.ExportDir.Note','')}</p>
 <button type="button" className={styles.primary} onClick={()=>send('openExportFolder')}>{t('Settings.OpenFolder','')}</button><p className={styles.exportPath}>{state.exportFolder}</p>
 <h3>{t('Settings.Service','')}</h3><p className={styles.muted}>{t('Settings.Service.Note','')}</p>
 <label>{t('Settings.Api','')}<input maxLength={1000} value={prefs.ApiEndpoint??'https://api.deepseek.com/chat/completions'} placeholder={t('Settings.Api.Placeholder','')} onChange={e=>edit('ApiEndpoint',e.target.value)}/></label><p className={styles.muted}>{t('Settings.Api.Note','')}</p>
 <label>{t('Settings.Model','')}<input value={prefs.Model??''} placeholder={t('Settings.Model.Placeholder','')} onChange={e=>edit('Model',e.target.value)}/></label>
 <button type="button" className={styles.check} role="switch" aria-checked={prefs.RequireApiKey!==false} onClick={()=>edit('RequireApiKey',prefs.RequireApiKey===false)}><span className={prefs.RequireApiKey!==false?styles.switchOn:styles.switchOff}><span/></span><span>{t('Settings.Auth','')}</span></button>
 <label>{t('Settings.Key','')}<input type="password" autoComplete="off" value={key} placeholder={state.keyConfigured&&prefs.ApiEndpoint===state.preferences?.ApiEndpoint?t('Settings.Key.Configured',''):t('Settings.Key.New','')} onChange={e=>setKey(e.target.value)}/></label><p className={styles.muted}>{t('Settings.Key.Note','')}</p>
 <button type="button" className={styles.disclosure} aria-expanded={advanced} onClick={()=>setAdvanced(!advanced)}>{t('Settings.Advanced','')} {advanced?'−':'+'}</button>{advanced&&<>
 <button type="button" className={styles.check} role="switch" aria-checked={prefs.JsonMode!==false} onClick={()=>edit('JsonMode',prefs.JsonMode===false)}><span className={prefs.JsonMode!==false?styles.switchOn:styles.switchOff}><span/></span><span>{t('Settings.JsonMode','')}</span></button><p className={styles.muted}>{t('Settings.JsonMode.Note','')}</p>
 <label>{t('Settings.MaxTokens','')}<input type="number" min={512} max={65536} value={prefs.MaxOutputTokens??8192} onChange={e=>edit('MaxOutputTokens',Number(e.target.value))}/></label>
 <label>{t('Settings.Timeout','')}<input type="number" min={15} max={600} value={prefs.TimeoutSeconds??180} onChange={e=>edit('TimeoutSeconds',Number(e.target.value))}/></label>
 <div className={styles.field}><span>{t('Settings.TokenParam','')}</span><div className={styles.scope}>{['max_tokens','max_completion_tokens'].map(value=><button type="button" key={value} aria-pressed={(prefs.TokenParameter??'max_tokens')===value} className={(prefs.TokenParameter??'max_tokens')===value?styles.active:''} onClick={()=>edit('TokenParameter',value)}>{value}</button>)}</div></div></>}
 <h3>{t('Settings.Commentary','')}</h3><button type="button" className={styles.check} role="switch" aria-checked={prefs.LongCommentary!==false} onClick={()=>edit('LongCommentary',prefs.LongCommentary===false)}><span className={prefs.LongCommentary!==false?styles.switchOn:styles.switchOff}><span/></span><span>{t('Settings.LongCommentary','')}</span></button>
 <div className={styles.field}><span>{t('Settings.Layout','')}</span><div className={styles.scope}>{layouts.map(([value,label])=><button type="button" key={value} aria-pressed={(prefs.LayoutMode??'auto')===value} className={(prefs.LayoutMode??'auto')===value?styles.active:''} onClick={()=>edit('LayoutMode',value)}>{label}</button>)}</div></div><p className={styles.muted}>{t('Settings.Layout.Note','')}</p>
 <label>{t('Settings.Editorial','')}<textarea rows={5} maxLength={6000} value={prefs.Editorial??''} onChange={e=>edit('Editorial',e.target.value)}/></label><p className={styles.muted}>{t('Settings.Editorial.Note','')}</p>
 <button className={styles.primary} onClick={()=>{send('configure',JSON.stringify({scope,preferences:prefs,apiKey:key}));setKey('');setDraft(null)}}>{t('Settings.Save','')}</button>
 <button type="button" className={styles.disclosure} aria-expanded={showCoverage} onClick={()=>setShowCoverage(!showCoverage)}>{t('Settings.Coverage','')} {showCoverage?'−':'+'}</button>{showCoverage&&<div className={styles.coverage}><p>{t('Settings.Coverage.Note','')}</p>{state.coverage?.map((x,i)=><div key={i}>{x}</div>)}</div>}</div>}
 </main></section>}</>;
}
// Keep a failure in this mod below the game's root React error boundary.
class NewsBoundary extends React.Component<{children:React.ReactNode;label:string},{failed:boolean}> {
 state={failed:false};
 static getDerivedStateFromError(){return {failed:true};}
 componentDidCatch(error:Error){console.error('[CityPress.UI] panel failed',error);}
 render(){return this.state.failed?<button type="button" className={styles.recovery} onClick={()=>this.setState({failed:false})}>{this.props.label}</button>:this.props.children;}
}
function BoundaryWithI18n({children}:{children:React.ReactNode}){const {t}=useI18n();return <NewsBoundary label={t('App.Recovery','')}>{children}</NewsBoundary>;}
const register:ModRegistrar=registry=>registry.append('GameTopLeft',()=> <BoundaryWithI18n><News/></BoundaryWithI18n>);
export default register;
