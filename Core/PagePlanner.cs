using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CityPress.Core {
 public sealed class PagePlanResult {
  public bool HasCommentary;
  public bool SplitPerformed;
  public NewspaperPage CommentaryPage;
  public List<NewspaperPage> Pages=new List<NewspaperPage>();
 }
 // Step 7 of the editorial process: deterministic page planning. The model may
 // write the copy, but it must not be able to collapse a day with several
 // domains into one "综合" page, nor to bury an independent commentary.
 public static class PagePlanner {
  public static readonly string[] ThemeOrder={"要闻","经济与财政","城市建设与交通","民生与社会","环境与观察","评论"};
  public const string FallbackTheme="城市要闻";
  static readonly string[] Economic={"财政","预算","收入","支出","结余","市库","资金","税","贷款","经济","商业","产业","就业","失业","补贴","投资","市场"};
  static readonly string[] Construction={"交通","公交","地铁","轨道","线路","车站","铁路","轮渡","航空","道路","建设","施工","工程","建筑","设施","用地","拆除","路网","停车","桥梁"};
  static readonly string[] Livelihood={"民生","社会","人口","家庭","教育","学校","医疗","医院","住宅","住房","游客","住宿","社区","服务","福利","就业","事故","火灾","治安","安全","损毁"};
  static readonly string[] Environment={"环境","生态","空气","污染","土壤","噪声","天气","气温","降雨","降雪","绿化","气候"};
  public static bool IsCommentary(Article article)=>article!=null&&article.Genre=="评论";
  public static bool IsOpinion(Article article)=>article!=null&&(article.Genre=="评论"||article.Genre=="分析");
  public static string Text(Article article)=>(article?.Section??"")+"\n"+(article?.Title??"")+"\n"+(article?.Body??"");
  public static bool IsGenericTheme(string theme) {
   if(string.IsNullOrWhiteSpace(theme))return true;
   var value=theme.Trim();
   if(value.Contains("综合"))return true;
   return value=="城市"||value=="新闻"||value=="城市新闻"||value=="今日"||value=="城市要闻";
  }
  public static int PageOrder(string theme) {
   if(string.IsNullOrWhiteSpace(theme))return 90;
   int index=Array.IndexOf(ThemeOrder,theme.Trim());
   return index<0?50:index;
  }
  static int Score(string text,string[] words)=>words.Count(word=>text.Contains(word));
  public static string ThemeOf(Article article) {
   if(article==null)return FallbackTheme;
   if(article.Genre=="评论")return "评论";
   var section=article.Section??"";
   var text=Text(article);
   if(Regex.IsMatch(section,"经济|财政|财经|产业|商业|市场|预算"))return "经济与财政";
   if(Regex.IsMatch(section,"交通|建设|城建|市政|运输|工程|道路"))return "城市建设与交通";
   if(Regex.IsMatch(section,"民生|社会|教育|医疗|人口|社区|服务|生活"))return "民生与社会";
   if(Regex.IsMatch(section,"环境|生态|气象|污染|绿色"))return "环境与观察";
   var scores=new[]{new{Theme="经济与财政",Value=Score(text,Economic)},new{Theme="城市建设与交通",Value=Score(text,Construction)},new{Theme="民生与社会",Value=Score(text,Livelihood)},new{Theme="环境与观察",Value=Score(text,Environment)}};
   var best=scores.OrderByDescending(x=>x.Value).First();
   return best.Value==0?FallbackTheme:best.Theme;
  }
  public static bool HasCommentary(Newspaper paper)=>PagesOf(paper).Any(page=>page.Articles.Count>0&&page.Articles.All(IsCommentary));
  public static List<NewspaperPage> PagesOf(Newspaper paper)=>paper?.Pages!=null&&paper.Pages.Count>0?paper.Pages.Where(page=>page!=null).ToList():PaperLayout.Pages(paper);
  static List<Article> DistinctCommentary(IEnumerable<Article> articles) {
   var result=new List<Article>();
   foreach(var article in articles){
    var body=(article.Body??"").Trim();
    if(result.Any(existing=>(existing.Title??"").Trim()==(article.Title??"").Trim()&&(existing.Body??"").Trim()==body))continue;
    result.Add(article);
   }
   return result;
  }
  static void DropEmptyCommentaryPage(List<NewspaperPage> pages) {
   for(int i=pages.Count-1;i>=0;i--)if(pages[i].Articles.Count==0)pages.RemoveAt(i);
  }
  // Moves every commentary article into one dedicated page and never deletes it.
  public static bool SeparateCommentary(Newspaper paper) {
   if(paper==null)return false;
   var pages=PagesOf(paper);var moved=new List<Article>();
   foreach(var page in pages){
    if(page.Articles==null)page.Articles=new List<Article>();
    if(!page.Articles.Any(IsCommentary))continue;
    moved.AddRange(page.Articles.Where(IsCommentary));
    page.Articles=page.Articles.Where(article=>!IsCommentary(article)).ToList();
   }
   DropEmptyCommentaryPage(pages);
   var commentary=DistinctCommentary(moved);
   if(commentary.Count>0){
    var target=pages.FirstOrDefault(page=>page.Articles.Count>0&&page.Articles.All(IsCommentary));
    if(target==null){target=new NewspaperPage{Theme="评论",Headline=commentary[0].Title??"评论",Deck="",Articles=commentary};pages.Add(target);}
    else target.Articles.AddRange(commentary.Where(article=>!target.Articles.Contains(article)));
   }
   paper.Pages=pages;paper.Articles=pages.SelectMany(page=>page.Articles).ToList();
   return commentary.Count>0;
  }
  public static void RemoveCommentary(Newspaper paper) {
   if(paper==null)return;var pages=PagesOf(paper);
   foreach(var page in pages)page.Articles=page.Articles.Where(article=>!IsCommentary(article)).ToList();
   DropEmptyCommentaryPage(pages);
   paper.Pages=pages;paper.Articles=pages.SelectMany(page=>page.Articles).ToList();
  }
  public static void AttachCommentary(Newspaper paper,Article commentary,string deck) {
   if(paper==null||commentary==null)return;
   RemoveCommentary(paper);
   var pages=PagesOf(paper);
   pages.Add(new NewspaperPage{Theme="评论",Headline=string.IsNullOrWhiteSpace(commentary.Title)?"评论":commentary.Title,Deck=deck??"",Articles=new List<Article>{commentary}});
   paper.Pages=pages;paper.Articles=pages.SelectMany(page=>page.Articles).ToList();
  }
  static string DominantTheme(IReadOnlyList<Article> articles) {
   var score=new Dictionary<string,int>();
   foreach(var article in articles)if(article.Genre!="评论"){
    var theme=ThemeOf(article);if(theme==FallbackTheme)continue;
    score[theme]=score.TryGetValue(theme,out var value)?value+1:1;
   }
   return score.Count==0?FallbackTheme:score.OrderByDescending(x=>x.Value).ThenBy(x=>x.Key,StringComparer.Ordinal).First().Key;
  }
  static List<NewspaperPage> BuildThemedPages(IReadOnlyList<Article> articles,EditorialBrief brief,string paperHeadline,string paperDeck) {
   var result=new List<NewspaperPage>();if(articles.Count==0)return result;
   var lead=articles.Where(article=>brief!=null&&brief.MatchesMajor(Text(article))).ToList();
   var remaining=articles.Where(article=>!lead.Contains(article)).ToList();
   var groups=new Dictionary<string,List<Article>>();
   foreach(var article in remaining){
    var theme=ThemeOf(article);if(theme==FallbackTheme)theme="要闻";
    if(!groups.TryGetValue(theme,out var list)){list=new List<Article>();groups[theme]=list;}list.Add(article);
   }
   var ordered=groups.Keys.Where(theme=>theme!="要闻").OrderBy(ThemeOrderIndex).ThenBy(theme=>theme,StringComparer.Ordinal).ToList();
   var leadArticles=new List<Article>(lead);
   if(groups.TryGetValue("要闻",out var unresolved))leadArticles.AddRange(unresolved);
   // A single-domain day should read as one themed page, not a fake front page.
   if(leadArticles.Count==0&&ordered.Count==1&&groups[ordered[0]].Count==articles.Count){
    var only=ordered[0];result.Add(new NewspaperPage{Theme=only,Headline=string.IsNullOrWhiteSpace(paperHeadline)?FirstTitle(groups[only]):paperHeadline,Deck=paperDeck??"",Articles=groups[only]});
    return result;
   }
   if(leadArticles.Count==0&&ordered.Count==0){
    leadArticles.AddRange(articles);
   }
   if(leadArticles.Count==0){leadArticles.Add(articles[0]);var removed=groups.Values.SelectMany(list=>list).Where(article=>article==articles[0]).ToList();foreach(var article in removed)groups.Values.ToList().ForEach(list=>list.Remove(article));ordered=groups.Keys.Where(theme=>groups[theme].Count>0).OrderBy(ThemeOrderIndex).ToList();}
   result.Add(new NewspaperPage{Theme="要闻",Headline=string.IsNullOrWhiteSpace(paperHeadline)?FirstTitle(leadArticles):paperHeadline,Deck=paperDeck??"",Articles=leadArticles});
   foreach(var theme in ordered){
    var list=groups[theme];
    // The leading article may already have been promoted to the front page.
    list=list.Where(article=>!leadArticles.Contains(article)).ToList();
    if(list.Count==0)continue;
    result.Add(new NewspaperPage{Theme=theme,Headline=list[0].Title??theme,Deck="",Articles=list});
   }
   return result;
  }
  static int ThemeOrderIndex(string theme)=>Math.Max(0,Array.IndexOf(ThemeOrder,theme)<0?50:Array.IndexOf(ThemeOrder,theme));
  static string FirstTitle(IReadOnlyList<Article> articles)=>articles.Count>0&&!string.IsNullOrWhiteSpace(articles[0].Title)?articles[0].Title:"城市新闻";
  static bool AllShort(IReadOnlyList<Article> articles)=>articles.All(article=>(article.Body??"").Length<260);
  static bool NeedsSplit(NewspaperPage page,EditorialBrief brief) {
   if(page==null||page.Articles.Count<3)return false;
   var themes=page.Articles.Where(article=>article.Genre!="评论").Select(ThemeOf).Where(theme=>theme!=FallbackTheme).Distinct().Count();
   if(themes<2)return false;
   if(brief!=null&&page.Articles.Any(article=>brief.MatchesMajor(Text(article))))return true;
   if(AllShort(page.Articles))return false; // a genuine brief-news digest is not a dumping ground
   return themes>=2&&page.Articles.Count>=3;
  }
  public static PagePlanResult Plan(Newspaper paper,EditorialBrief brief) {
   var result=new PagePlanResult();if(paper==null)return result;
   paper.Articles=paper.Articles??new List<Article>();
   var pages=PagesOf(paper);
   // 1) pull commentary out of every mixed page
   var commentary=new List<Article>();string commentaryDeck="";
   var newsPages=new List<NewspaperPage>();
   foreach(var page in pages){
    page.Articles=page.Articles??new List<Article>();
    page.Articles=page.Articles.Where(article=>article!=null).ToList();
    var comments=page.Articles.Where(IsCommentary).ToList();
    if(comments.Count>0){
     commentary.AddRange(comments);
     if(string.IsNullOrWhiteSpace(commentaryDeck)&&!string.IsNullOrWhiteSpace(page.Deck))commentaryDeck=page.Deck;
     page.Articles=page.Articles.Where(article=>!IsCommentary(article)).ToList();
    }
    if(page.Articles.Count==0)continue;
    newsPages.Add(page);
   }
   // 2) split generic/one-page dumps into themed pages
   var planned=new List<NewspaperPage>();
   foreach(var page in newsPages){
    var generic=IsGenericTheme(page.Theme)||string.IsNullOrWhiteSpace(page.Theme);
    if(generic&&NeedsSplit(page,brief)){
     result.SplitPerformed=true;
     foreach(var themed in BuildThemedPages(page.Articles,brief,paper.Headline,paper.Deck))planned.Add(themed);
     continue;
    }
    if(generic){
     var dominant=DominantTheme(page.Articles);
     bool singleTheme=page.Articles.All(article=>{var theme=ThemeOf(article);return theme==dominant||theme==FallbackTheme;});
     if(dominant!=FallbackTheme&&singleTheme)page.Theme=dominant;
     else if(page.Articles.Count==1)page.Theme=ThemeOf(page.Articles[0]);
    }
    if(string.IsNullOrWhiteSpace(page.Headline))page.Headline=FirstTitle(page.Articles);
    page.Deck=page.Deck??"";
    planned.Add(page);
   }
   if(planned.Count==1&&IsGenericTheme(planned[0].Theme)&&planned[0].Articles.Count>=3&&NeedsSplit(planned[0],brief)){
    var only=planned[0];planned=BuildThemedPages(only.Articles,brief,paper.Headline,paper.Deck);result.SplitPerformed=true;
   }
   // 3) merge pages that ended up with the same theme, keeping original order
   var merged=new List<NewspaperPage>();
   foreach(var page in planned){
    var existing=merged.FirstOrDefault(item=>item.Theme==page.Theme);
    if(existing==null){merged.Add(page);continue;}
    foreach(var article in page.Articles)if(!existing.Articles.Contains(article))existing.Articles.Add(article);
    if(string.IsNullOrWhiteSpace(existing.Headline)&&!string.IsNullOrWhiteSpace(page.Headline))existing.Headline=page.Headline;
   }
   merged=merged.OrderBy(page=>PageOrder(page.Theme)).ThenBy(page=>page.Theme,StringComparer.Ordinal).ToList();
   // 4) keep the paper readable: merge the smallest extra pages instead of dropping copy
   while(merged.Count>6){
    var smallest=merged.Where(page=>page.Theme!="要闻").OrderBy(page=>page.Articles.Count).ThenByDescending(page=>PageOrder(page.Theme)).FirstOrDefault();
    if(smallest==null)break;
    var target=merged.FirstOrDefault(page=>page!=smallest&&page.Theme==smallest.Theme)??merged.FirstOrDefault(page=>page!=smallest&&page.Theme=="要闻")??merged.Where(page=>page!=smallest&&page.Theme!="评论").OrderBy(page=>PageOrder(page.Theme)).FirstOrDefault();
    if(target==null)break;
    foreach(var article in smallest.Articles)target.Articles.Add(article);
    merged.Remove(smallest);
   }
   // 5) the commentary always owns an independent edition at the end
   var commentaryArticles=DistinctCommentary(commentary);
   if(commentaryArticles.Count>0){
    var page=new NewspaperPage{Theme="评论",Headline=commentaryArticles[0].Title??"评论",Deck=commentaryDeck,Articles=commentaryArticles};
    merged.Add(page);result.CommentaryPage=page;result.HasCommentary=true;
   }
   paper.Pages=merged;paper.Articles=merged.SelectMany(page=>page.Articles).ToList();result.Pages=merged;
   return result;
  }
  // Used when the dedicated commentary request returns after the news pass.
  public static PagePlanResult PlanWithCommentary(Newspaper paper,Article commentary,string deck,EditorialBrief brief) {
   AttachCommentary(paper,commentary,deck);
   return Plan(paper,brief);
  }
 }
}
