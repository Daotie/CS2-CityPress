import spec from '../../Resources/paper-layout.json';
export function groupArticles<T extends {Title:string;Body:string}>(articles:T[]):T[][] {
 const count=Math.max(1,Math.min(articles.length,articles.length<3?spec.sparseColumns:spec.maxColumns));
 const weights=articles.map(a=>(a.Body??'').length+(a.Title??'').length*spec.titleWeight+spec.articleOverhead);
 let remaining=weights.reduce((a,b)=>a+b,0),cursor=0;const groups:T[][]=[];
 for(let column=0;column<count;column++){
  const group:T[]=[];const target=remaining/(count-column);let used=0;
  while(cursor<articles.length){
   if(group.length>0&&(articles.length-cursor<=count-column-1||Math.abs(used-target)<=Math.abs(used+weights[cursor]-target)))break;
   used+=weights[cursor];group.push(articles[cursor++]);
  }
  remaining-=used;groups.push(group);
 }
 return groups;
}

export function editionPages<T extends {Section:string;Title:string;Body:string;Genre?:string}>(paper:{Headline:string;Deck:string;Articles:T[];Pages?:{Theme:string;Headline:string;Deck:string;Articles:T[]}[]}) {
 return paper.Pages?.length?paper.Pages:[{Theme:'综合',Headline:paper.Headline,Deck:paper.Deck,Articles:paper.Articles??[]}];
}

export interface LayoutBlock {Kind:string;Article:number;Start:number;Count:number}
export interface PageComposition {Template:string;Top:LayoutBlock[];Columns:{Width:number;Blocks:LayoutBlock[]}[]}
interface ComposedPage {Theme:string;Headline:string;Deck:string;Articles:{Section:string;Title:string;Body:string;Genre?:string}[];Composition?:PageComposition}
export function paragraphs(text:string):string[]{
 const lines=(text??'').split('\n').map(line=>line.trim()).filter(Boolean);if(lines.length!==1||lines[0].length<=450)return lines;
 const result:string[]=[];let current='';for(const char of lines[0]){current+=char;if('。！？'.includes(char)&&current.length>=220){result.push(current);current='';}}if(current)result.push(current);return result;
}
export function isSubhead(text:string):boolean{return (text??'').trimStart().startsWith('## ');}
export function subheadText(text:string):string{const value=(text??'').trim();return value.startsWith('## ')?value.slice(3).trim():value;}
export function blockText(page:ComposedPage,block:LayoutBlock):string{
 if(block.Kind==='headline'||block.Kind==='vertical-headline')return page.Headline??'';if(block.Kind==='deck')return page.Deck??'';
 const story=page.Articles[block.Article];if(!story)return '';
 if(block.Kind==='section')return (story.Section??page.Theme)+(story.Genre==='评论'||story.Genre==='分析'?' · '+story.Genre:'');if(block.Kind==='title')return story.Title??'';
 const lines=paragraphs(story.Body).slice(block.Start);
 if(block.Kind==='subhead')return lines.length?subheadText(lines[0]):'';
 return (block.Count<0?lines:lines.slice(0,block.Count)).join('\n');
}
export function compositionFor(page:ComposedPage):PageComposition{
 if(page.Composition?.Columns?.length){
  if(page.Composition.Template!=='vertical-lead'||(page.Headline.length<=20&&!/[0-9A-Za-z，,；;：:。\s]/.test(page.Headline)))return page.Composition;
 }
 const b=(Kind:string,Article=-1):LayoutBlock=>({Kind,Article,Start:0,Count:-1});let index=0;
 return {Template:'banner',Top:[b('headline'),b('deck')],Columns:groupArticles(page.Articles??[]).map(group=>({Width:1,Blocks:group.reduce<LayoutBlock[]>((all)=>{const n=index++;all.push(b('section',n),b('title',n),b('body',n));return all;},[])}))};
}
export function verticalColumns(text:string):string[][]{const chars=Array.from(text??''),maximum=spec.templates['vertical-lead'].glyphsPerColumn,columnCount=Math.max(1,Math.ceil(chars.length/maximum)),count=Math.max(1,Math.ceil(chars.length/columnCount));const result:string[][]=[];for(let i=0;i<chars.length;i+=count)result.push(chars.slice(i,i+count));return result;}
