using System;
using System.Collections.Generic;
using System.Linq;
namespace CityPress.Core {
 // Deterministic last-mile reporting uses only the prepared, verified facts.
 // It does not fabricate a long opinion merely to satisfy a length check.
 public static class PublicationRepair {
  public static void Complete(Newspaper paper,DayLog day){
   paper.Articles=paper.Articles??new List<Article>();
   bool Covered(string text)=>paper.Articles.Any(a=>a.Genre!="评论"&&a.Genre!="分析"&&(a.Title+"\n"+a.Body).Contains(text));
   void Add(string id,string section,string title,string body){
    if(paper.Articles.Any(a=>a.Id==id))return;
    var article=new Article{Id=id,Section=section,Genre="简讯",Title=title,Body=body,Status="repaired",Revision=1};
    paper.Articles.Add(article);
    if(paper.Pages!=null)paper.Pages.Add(new NewspaperPage{Theme=PagePlanner.ThemeOf(article),Headline=title,Deck="",Articles=new List<Article>{article}});
   }
   foreach(var line in day.Transit??new List<TransitRecord>()){
    if(line==null||string.IsNullOrWhiteSpace(line.Name)||string.IsNullOrWhiteSpace(line.Change))continue;
    bool opening=line.Change.Contains("投入运营"),resume=line.Change.Contains("恢复运营");
    if(!opening&&!resume)continue;
    // A name in an inventory or an opinion is insufficient evidence of coverage.
    string verb=opening?"投入运营":"恢复运营";
    if(paper.Articles.Any(a=>a.Genre!="评论"&&a.Genre!="分析"&&(a.Title+"\n"+a.Body).Contains(line.Name)&&(a.Title+"\n"+a.Body).Contains(verb)))continue;
    Add("repair:transit:"+line.Id,"交通",line.Name+verb,line.Name+"已"+verb+"。"+(line.Fare>=0?"线路票价为"+line.Fare+"元。":""));
   }
   foreach(var item in day.Breakthroughs??new List<BreakthroughRecord>()){
    if(item==null||string.IsNullOrWhiteSpace(item.Label)||Covered(item.Label))continue;
    Add("repair:milestone:"+item.Key,"城市要闻",item.Label+"达到新规模",item.Label+"达到"+EditorialMaterial.Display(item.Value,item.Unit)+"，跨过"+EditorialMaterial.Display(item.Threshold,item.Unit)+"。" );
   }
   if(paper.Articles.Any(a=>a.Genre=="消息"||a.Genre=="简讯"))return;
   foreach(var m in day.Metrics??new List<MetricRecord>()){
    if(m==null||string.IsNullOrWhiteSpace(m.Label)||m.Samples<2||m.First==m.Last)continue;
    Add("repair:metric:"+m.Key,"城市动态",m.Label+"出现变化",m.Label+"由"+EditorialMaterial.Display(m.First,m.Unit)+"变为"+EditorialMaterial.Display(m.Last,m.Unit)+"。");
   }
   if(paper.Articles.Any(a=>a.Genre=="简讯"))return;
   foreach(var a in EditorialInput.Select(day)){
    if(string.IsNullOrWhiteSpace(a.Detail))continue;
    Add("repair:action:"+Storage.Hash(a.Kind+"|"+a.Detail),"城市动态","城市建设与服务动态",a.Detail+"。" );
   }
   if(paper.Articles.Count==0){paper.LocalOnly=true;paper.Headline=I18n.Say("本期简讯","Local brief");paper.Deck=I18n.Say("本期暂无可刊发的新消息。","No reportable updates for this edition.");}
  }
 }
}
