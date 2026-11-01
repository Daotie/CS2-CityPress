using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace CityPress.Core {
 public static class EditionRules {
  // These are editorial-quality signals, not a reason to discard a whole issue.
  // The pipeline asks the model to rewrite flagged copy once, then removes any
  // remaining cue sentences deterministically before layout.
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
  public static bool ContainsProcessPhrase(string text)=>!string.IsNullOrEmpty(text)&&processPhrases.Any(text.Contains);
  public static bool HasProcessPhrases(Newspaper paper){
   if(paper==null)return false;
   return ContainsProcessPhrase(paper.Headline)||ContainsProcessPhrase(paper.Deck)||PagePlanner.PagesOf(paper).Any(page=>ContainsProcessPhrase(page.Headline)||ContainsProcessPhrase(page.Deck)||page.Articles.Any(article=>ContainsProcessPhrase(article.Title)||ContainsProcessPhrase(article.Body)));
  }
  static string Clean(string text,ref int removed){
   if(string.IsNullOrWhiteSpace(text))return text;
   var lines=new List<string>();
   foreach(var source in text.Split('\n')){
    var sentences=Regex.Split(source,@"(?<=[。！？!?])");
    var kept=sentences.Where(sentence=>!ContainsProcessPhrase(sentence)).ToList();
    removed+=sentences.Length-kept.Count;
    var line=string.Concat(kept).Trim();
    if(line.Length>0)lines.Add(line);
   }
   return string.Join("\n",lines);
  }
  // Removes sentences that expose the production process or caveat phrasing.
  // Returns the number of removed sentences. Titles fall back to the cleaned
  // first sentence of the body so an issue is not dropped just for one cue.
  public static int Sanitize(Newspaper paper){
   if(paper==null)return 0;int removed=0;
   paper.Headline=Clean(paper.Headline,ref removed);
   paper.Deck=Clean(paper.Deck,ref removed);
   foreach(var page in PagePlanner.PagesOf(paper)){
    page.Headline=Clean(page.Headline,ref removed);
    page.Deck=Clean(page.Deck,ref removed);
    foreach(var article in page.Articles){
     article.Body=Clean(article.Body,ref removed);
     article.Title=Clean(article.Title,ref removed);
     if(string.IsNullOrWhiteSpace(article.Title)){
      var bodyLine=(article.Body??"").Split('\n').FirstOrDefault(line=>!string.IsNullOrWhiteSpace(line));
      article.Title=bodyLine==null?"新闻":Truncate(bodyLine.Trim(),30);
     }
    }
   }
   paper.Articles=PagePlanner.PagesOf(paper).SelectMany(page=>page.Articles).ToList();
   return removed;
  }
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
   // Process/caveat phrases are handled by QualityControl and a targeted
   // revision. Never reject the whole issue here.
  }
 }
}
