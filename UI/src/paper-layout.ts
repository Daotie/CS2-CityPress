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
