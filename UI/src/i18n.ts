import {useLocalization} from 'cs2/l10n';
import enUS from '../../lang/en-US.json';
import zhHANS from '../../lang/zh-HANS.json';

type Dictionary=Record<string,string>;
const en=enUS as unknown as Dictionary;
const zh=zhHANS as unknown as Dictionary;
const UI_PREFIX='CityPress.UI.';
const CONTINUATION=/（续(?:[一二三四五六七八九])?）$/;
const THEME_KEYS:Record<string,string>={
 '综合':'Theme.General','要闻':'Theme.Front','经济与财政':'Theme.Economy','城市建设与交通':'Theme.Construction',
 '民生与社会':'Theme.Livelihood','环境与观察':'Theme.Environment','评论':'Theme.Opinion','城市要闻':'Theme.CityNews'
};

function format(template:string,args:Array<string|number>):string{
 if(!args||args.length===0)return template;
 return template.replace(/\{(\d+)\}/g,(match,index:string)=>{const value=args[Number(index)];return value===undefined?match:String(value);});
}

export interface I18n {
 t:(id:string,fallback:string,...args:Array<string|number>)=>string;
 tr:(key:string,...args:Array<string|number>)=>string;
 isChinese:boolean;
 themeLabel:(theme:string)=>string;
 date:(day:string)=>string;
}

// The game dictionary normally provides these strings. The imported tables are
// a built-in fallback so an older install or an unsupported game language never
// renders raw translation ids.
export function useI18n():I18n {
 const {translate}=useLocalization();
 const locale=translate('CityPress.Locale','en-US')||'en-US';
 const isChinese=locale.toLowerCase().startsWith('zh');
 const dict=isChinese?zh:en;
 const tr=(key:string,...args:Array<string|number>)=>{
  const fallback=dict[key];
  const value=translate(key,fallback)||fallback||key;
  return format(value,args);
 };
 const t=(id:string,fallback:string,...args:Array<string|number>)=>{
  const key=UI_PREFIX+id;
  const value=translate(key,dict[key]??fallback)||dict[key]||fallback||id;
  return format(value,args);
 };
 const themeLabel=(theme:string)=>{
  if(!theme)return tr('CityPress.UI.Theme.General');
  let base=theme.trim();let continuation=0;
  while(CONTINUATION.test(base)){continuation++;base=base.replace(CONTINUATION,'').trim();}
  const key=THEME_KEYS[base];
  let label=key?tr(UI_PREFIX+key):base;
  for(let i=0;i<continuation;i++)label+=tr('CityPress.UI.Theme.Continued');
  return label;
 };
 const date=(day:string)=>{
  const match=/^(\d{4})-(\d{2})-(\d{2})$/.exec(day);
  if(!match)return day;
  const result=tr('CityPress.UI.Date.Pattern',match[1],match[2],match[3]);
  const time=new Date(day+'T12:00:00Z');
  if(isNaN(time.getTime()))return result;
  return result+' '+tr('CityPress.UI.Date.Weekday.'+time.getUTCDay());
 };
 return {t,tr,isChinese,themeLabel,date};
}
