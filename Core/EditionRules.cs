using System;
using System.Collections.Generic;
using System.Linq;
namespace CityPress.Core {
 public static class EditionRules {
  static readonly string[] processPhrases={"施工标记","仍待观察","实际服务情况待观察","不代表完整自然日","观测区间","不等同于","制作过程","原生分组","采集口径","以游戏为准","游戏内","玩家操作","模组","存档","原生单位","实体编号","报道未获知"};
  static readonly string[] genres={"消息","简讯","评论","分析"};
  static string NormalizeGenre(string genre) {
   if(string.IsNullOrWhiteSpace(genre))return "消息";
   var value=genre.Trim();
   if(genres.Contains(value))return value;
   if(value.Contains("评论")||value.Contains("社论")||value.Contains("观点"))return "评论";
   if(value.Contains("分析")||value.Contains("解读"))return "分析";
   if(value.Contains("简讯")||value.Contains("短讯"))return "简讯";
   return "消息";
  }
  static string Truncate(string text,int length)=>text==null?null:text.Length<=length?text:text.Substring(0,length);
  // Moves commentary into its own edition. Kept for compatibility; the full
  // deterministic page plan is PagePlanner.Plan.
  public static void SeparateCommentary(Newspaper paper){
   if(paper==null)return;
   PagePlanner.SeparateCommentary(paper);
   Normalize(paper);
  }
  public static void Normalize(Newspaper paper) {
   if(paper==null||string.IsNullOrWhiteSpace(paper.Headline)||paper.Headline.Length>300)throw new InvalidOperationException("返回的报纸标题无效，未存入缓存。");
   paper.Headline=paper.Headline.Trim();paper.Deck=paper.Deck??"";
   if(paper.Pages!=null&&paper.Pages.Count>0){
    var pages=new List<NewspaperPage>();
    foreach(var page in paper.Pages){
     if(page==null)continue;
     page.Articles=page.Articles??new List<Article>();
     page.Articles=page.Articles.Where(article=>article!=null).ToList();
     if(page.Articles.Count==0)continue;
     page.Theme=string.IsNullOrWhiteSpace(page.Theme)?"综合":Truncate(page.Theme.Trim(),24);
     page.Headline=string.IsNullOrWhiteSpace(page.Headline)?(page.Articles[0].Title??paper.Headline):Truncate(page.Headline.Trim(),300);
     page.Deck=Truncate(page.Deck??"",600);
     pages.Add(page);
    }
    if(pages.Count>12)throw new InvalidOperationException("返回的分版结构无效，未存入缓存。");
    paper.Pages=pages;
    paper.Articles=pages.SelectMany(page=>page.Articles).ToList();
   }
   if(paper.Articles==null)paper.Articles=new List<Article>();
   if(paper.Articles.Count>24||paper.Articles.Any(article=>string.IsNullOrWhiteSpace(article.Title)||string.IsNullOrWhiteSpace(article.Body)||article.Title.Length>300||article.Body.Length>12000)||paper.Articles.Sum(article=>article.Body.Length)>80000)throw new InvalidOperationException("返回的报纸正文无效，未存入缓存。");
   foreach(var article in paper.Articles){article.Title=article.Title.Trim();article.Genre=NormalizeGenre(article.Genre);article.Section=string.IsNullOrWhiteSpace(article.Section)?null:article.Section.Trim();}
   var copy=string.Join("\n",PagePlanner.PagesOf(paper).Select(page=>page.Theme+"\n"+page.Headline+"\n"+page.Deck))+"\n"+paper.Headline+"\n"+paper.Deck+"\n"+string.Join("\n",paper.Articles.Select(article=>article.Title+"\n"+article.Body));
   if(processPhrases.Any(copy.Contains))throw new InvalidOperationException("稿件含制作口径或模板式免责措辞，未缓存；请重新编印。");
  }
 }
}
