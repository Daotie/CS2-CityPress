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
 // domains into one "综合" page, nor to bury an independent commentary. A page
 // is not limited to one topic: thin groups share a page, rich groups continue
 // onto extra editions when the material needs the space.
 public static class PagePlanner {
  public static readonly string[] ThemeOrder={"要闻","经济与财政","城市建设与交通","民生与社会","环境与观察","评论"};
  public const string FallbackTheme="城市要闻";
  const string ContinuationSuffix="（续）";
  static readonly Regex ContinuationPattern=new Regex(@"（续([一二三四五六七八九]?)）$",RegexOptions.CultureInvariant);
  static readonly string[] ChineseDigits={"","一","二","三","四","五","六","七","八","九"};
  static readonly string[] Economic={"财政","预算","收入","支出","结余","市库","资金","税","贷款","经济","商业","产业","就业","失业","补贴","投资","市场"};
  static readonly string[] Construction={"交通","公交","地铁","轨道","线路","车站","铁路","轮渡","航空","道路","建设","施工","工程","建筑","设施","用地","拆除","路网","停车","桥梁"};
  static readonly string[] Livelihood={"民生","社会","人口","家庭","教育","学校","医疗","医院","住宅","住房","游客","住宿","社区","服务","福利","就业","事故","火灾","治安","安全","损毁"};
  static readonly string[] Environment={"环境","生态","空气","污染","土壤","噪声","天气","气温","降雨","降雪","绿化","气候"};
  public static bool IsCommentary(Article article)=>article!=null&&article.Genre=="评论";
  public static bool IsOpinion(Article article)=>article!=null&&(article.Genre=="评论"||article.Genre=="分析");
  public static string Text(Article article)=>(article?.Section??"")+"\n"+(article?.Title??"")+"\n"+(article?.Body??"");
  public static bool IsGenericTheme(string theme) {
   if(string.IsNullOrWhiteSpace(theme))return true;
   var value=BaseTheme(theme);
   if(value.Contains("综合"))return true;
   return value=="城市"||value=="新闻"||value=="城市新闻"||value=="今日"||value=="城市要闻";
  }
  public static string BaseTheme(string theme) {
   if(string.IsNullOrWhiteSpace(theme))return theme;
   var value=theme.Trim();
   while(ContinuationPattern.IsMatch(value))value=ContinuationPattern.Replace(value,"").Trim();
   return value;
  }
  public static int PageOrder(string theme) {
   if(string.IsNullOrWhiteSpace(theme))return 90;
   int index=Array.IndexOf(ThemeOrder,BaseTheme(theme));
   return index<0?50:index;
  }
  static int ContinuationIndex(string theme) {
   if(string.IsNullOrWhiteSpace(theme))return 0;
   var match=ContinuationPattern.Match(theme.Trim());if(!match.Success)return 0;
   var digit=match.Groups[1].Value;return digit.Length==0?1:Array.IndexOf(ChineseDigits,digit);
  }
  static string ThemeWithContinuation(string theme,int index)=>index<=0?theme:theme+"（续"+(index==1?"":ChineseDigits[Math.Min(index,8)])+"）";
  static int Estimate(Article article)=>(article?.Body??"").Length+(article?.Title??"").Length*2+60;
  static int Estimate(NewspaperPage page)=>page?.Articles?.Sum(Estimate)??0;
  static int Estimate(IEnumerable<Article> articles)=>articles.Sum(Estimate);
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
   var commentary=OrderCommentary(moved);
   if(commentary.Count>0){
    var target=pages.FirstOrDefault(page=>page.Articles.Count>0&&page.Articles.All(IsCommentary));
    if(target==null){target=new NewspaperPage{Theme="评论",Headline=commentary[0].Title??"评论",Deck="",Articles=commentary};pages.Add(target);}
    else target.Articles.AddRange(commentary.Where(article=>!target.Articles.Contains(article)));
   }
   paper.Pages=pages;paper.Articles=pages.SelectMany(page=>page.Articles).ToList();
   return commentary.Count>0;
  }
  static List<Article> OrderCommentary(IEnumerable<Article> articles) {
   var ordered=articles.Where(article=>article!=null).OrderByDescending(article=>PaperLayout.BodyLength(article.Body)).ToList();
   for(int i=0;i<ordered.Count;i++)ordered[i].CommentaryRole=i==0||PaperLayout.BodyLength(ordered[i].Body)>=450?"长评":"短评";
   return ordered;
  }
  public static void RemoveCommentary(Newspaper paper) {
   if(paper==null)return;var pages=PagesOf(paper);
   foreach(var page in pages)page.Articles=page.Articles.Where(article=>!IsCommentary(article)).ToList();
   DropEmptyCommentaryPage(pages);
   paper.Pages=pages;paper.Articles=pages.SelectMany(page=>page.Articles).ToList();
  }
  public static void AttachCommentary(Newspaper paper,Article lead,string deck,IEnumerable<Article> shortComments=null) {
   if(paper==null||lead==null)return;
   lead.CommentaryRole="长评";
   RemoveCommentary(paper);
   var pages=PagesOf(paper);var articles=new List<Article>{lead};
   if(shortComments!=null)articles.AddRange(shortComments.Where(article=>article!=null));
   foreach(var extra in articles.Skip(1))extra.CommentaryRole=PaperLayout.BodyLength(extra.Body)>=450?"长评":"短评";
   pages.Add(new NewspaperPage{Theme="评论",Headline=string.IsNullOrWhiteSpace(lead.Title)?"评论":lead.Title,Deck=deck??"",Articles=articles});
   paper.Pages=pages;paper.Articles=pages.SelectMany(page=>page.Articles).ToList();
  }
  public static PagePlanResult PlanWithCommentary(Newspaper paper,Article commentary,string deck,EditorialBrief brief) {
   AttachCommentary(paper,commentary,deck);
   return Plan(paper,brief);
  }
  static string DominantTheme(IReadOnlyList<Article> articles) {
   var score=new Dictionary<string,int>();
   foreach(var article in articles)if(article.Genre!="评论"){
    var theme=ThemeOf(article);if(theme==FallbackTheme)continue;
    score[theme]=score.TryGetValue(theme,out var value)?value+1:1;
   }
   return score.Count==0?FallbackTheme:score.OrderByDescending(x=>x.Value).ThenBy(x=>x.Key,StringComparer.Ordinal).First().Key;
  }
  // Splits a theme's articles into readable editions. One edition is not forced
  // to hold a whole category: when a category has enough copy, it continues on
  // an extra page (with a（续）label) instead of being crammed or dropped.
  static List<NewspaperPage> ChunkTheme(string theme,IReadOnlyList<Article> articles,string headline,string deck) {
   var pages=new List<NewspaperPage>();if(articles.Count==0)return pages;
   var buffer=new List<Article>();int size=0;
   void Flush(){
    if(buffer.Count==0)return;
    var first=buffer[0];
    pages.Add(new NewspaperPage{Theme=ThemeWithContinuation(theme,pages.Count),Headline=pages.Count==0&&!string.IsNullOrWhiteSpace(headline)?headline:(string.IsNullOrWhiteSpace(first.Title)?theme:first.Title),Deck=pages.Count==0?deck??"":"",Articles=new List<Article>(buffer)});
    buffer.Clear();size=0;
   }
   foreach(var article in articles){int weight=Estimate(article);if(buffer.Count>0&&size+weight>PaperLayout.PageCapacity)Flush();buffer.Add(article);size+=weight;}
   Flush();return pages;
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
   if(leadArticles.Count==0&&ordered.Count==1&&groups[ordered[0]].Count==articles.Count){
    var only=ordered[0];return ChunkTheme(only,groups[only],paperHeadline,paperDeck);
   }
   if(leadArticles.Count==0&&ordered.Count==0)leadArticles.AddRange(articles);
   if(leadArticles.Count==0){leadArticles.Add(articles[0]);foreach(var list in groups.Values)list.Remove(articles[0]);ordered=groups.Keys.Where(theme=>groups[theme].Count>0).OrderBy(ThemeOrderIndex).ToList();}
   result.AddRange(ChunkTheme("要闻",leadArticles,paperHeadline,paperDeck));
   foreach(var theme in ordered){
    var list=groups[theme].Where(article=>!leadArticles.Contains(article)).ToList();
    if(list.Count==0)continue;
    result.AddRange(ChunkTheme(theme,list,null,""));
   }
   return result;
  }
  static int ThemeOrderIndex(string theme)=>Math.Max(0,Array.IndexOf(ThemeOrder,BaseTheme(theme))<0?50:Array.IndexOf(ThemeOrder,BaseTheme(theme)));
  static bool AllShort(IReadOnlyList<Article> articles)=>articles.All(article=>(article.Body??"").Length<260);
  static bool NeedsSplit(NewspaperPage page,EditorialBrief brief) {
   if(page==null||page.Articles.Count<3)return false;
   var themes=page.Articles.Where(article=>article.Genre!="评论").Select(ThemeOf).Where(theme=>theme!=FallbackTheme).Distinct().Count();
   if(themes<2)return false;
   if(brief!=null&&page.Articles.Any(article=>brief.MatchesMajor(Text(article))))return true;
   if(AllShort(page.Articles))return false;
   return themes>=2&&page.Articles.Count>=3;
  }
  static List<NewspaperPage> SplitOversized(NewspaperPage page) {
   if(page.Articles.Count<2||Estimate(page)<=PaperLayout.PageCapacity*1.2)return new List<NewspaperPage>{page};
   var pages=new List<NewspaperPage>();var buffer=new List<Article>();int size=0;
   void Flush(){
    if(buffer.Count==0)return;
    var first=buffer[0];
    pages.Add(new NewspaperPage{Theme=ThemeWithContinuation(BaseTheme(page.Theme),pages.Count),Headline=pages.Count==0?page.Headline:(string.IsNullOrWhiteSpace(first.Title)?BaseTheme(page.Theme):first.Title),Deck=pages.Count==0?page.Deck??"":"",Articles=new List<Article>(buffer)});
    buffer.Clear();size=0;
   }
   foreach(var article in page.Articles){int weight=Estimate(article);if(buffer.Count>0&&size+weight>PaperLayout.PageCapacity)Flush();buffer.Add(article);size+=weight;}
   Flush();
   if(pages.Count>0)pages[0].Headline=page.Headline??pages[0].Headline;
   return pages;
  }
  static List<NewspaperPage> OrderPages(IEnumerable<NewspaperPage> pages)=>pages.Select((page,index)=>new{page,index}).OrderBy(item=>PageOrder(item.page.Theme)).ThenBy(item=>ContinuationIndex(item.page.Theme)).ThenBy(item=>item.index).Select(item=>item.page).ToList();
  public static void AddFrontPageGuide(Newspaper paper,EditorialBrief brief) {
   var pages=PagesOf(paper);
   var front=pages.FirstOrDefault(page=>BaseTheme(page.Theme)=="要闻");
   bool created=false;
   if(front==null){
    if(pages.Count(page=>page.Articles.Any(a=>a.Genre!="评论"))<3)return;
    front=new NewspaperPage{Theme="要闻",Headline=paper.Headline,Deck=""};pages.Insert(0,front);created=true;
   }
   front.Articles.RemoveAll(article=>article.Section=="本期导读");
   bool english=!I18n.MostlyChinese((paper.Headline??"")+string.Join("",pages.SelectMany(p=>p.Articles).Select(a=>a.Body)));
   var entries=new List<string>();
   for(int i=0;i<pages.Count;i++){
    var page=pages[i];if(page==front||BaseTheme(page.Theme)=="评论"||page.Articles.Count==0)continue;
    var lead=page.Articles[0];
    bool important=(brief!=null&&brief.MatchesMajor(Text(lead)))||page.Articles.Sum(a=>(a.Body??"").Length)>=400;
    if(!important)continue;
    string summary=Regex.Split(lead.Body??"",@"(?<=[。！？])").FirstOrDefault(line=>!string.IsNullOrWhiteSpace(line))?.Trim();
    if(string.IsNullOrEmpty(summary)||summary.Length>110)summary=lead.Title;
    entries.Add(english?summary.TrimEnd('.')+". See page "+(i+1)+" · "+page.Theme+".":summary.TrimEnd('。','！','？')+"。详见第"+(i+1)+"版·"+page.Theme+"。");
   }
   if(entries.Count==0&&created)pages.Remove(front);
   if(entries.Count>0)front.Articles.Add(new Article{Section="本期导读",Genre="简讯",Title=english?"Inside this issue":"本期导读",Body=string.Join("\n",entries),Status="passed"});
   paper.Pages=pages;paper.Articles=pages.SelectMany(page=>page.Articles).ToList();
  }
  public static PagePlanResult Plan(Newspaper paper,EditorialBrief brief) {
   var result=new PagePlanResult();if(paper==null)return result;
   paper.Articles=paper.Articles??new List<Article>();
   var pages=PagesOf(paper);
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
    if(string.IsNullOrWhiteSpace(page.Headline))page.Headline=page.Articles.Count>0?(page.Articles[0].Title??BaseTheme(page.Theme)):BaseTheme(page.Theme);
    page.Deck=page.Deck??"";
    foreach(var split in SplitOversized(page))if(!planned.Contains(split))planned.Add(split);
   }
   if(planned.Count==1&&IsGenericTheme(planned[0].Theme)&&planned[0].Articles.Count>=3&&NeedsSplit(planned[0],brief)){
    var only=planned[0];planned=BuildThemedPages(only.Articles,brief,paper.Headline,paper.Deck);result.SplitPerformed=true;
   }
   // Merge same-topic editions only while they still fit on one page.
   var merged=new List<NewspaperPage>();
   foreach(var page in planned){
    var existing=merged.FirstOrDefault(item=>item.Theme==page.Theme);
    if(existing!=null&&Estimate(existing)+Estimate(page)<=PaperLayout.PageCapacity){
     existing.Articles.AddRange(page.Articles);
     if(string.IsNullOrWhiteSpace(existing.Headline)&&!string.IsNullOrWhiteSpace(page.Headline))existing.Headline=page.Headline;
     continue;
    }
    merged.Add(page);
   }
   // Thin leftover editions join another page only when the paper has too many
   // pages; otherwise themed pages are preserved even when the topic is small.
   merged=OrderPages(merged);
   while(merged.Count>PaperLayout.MaxNewsPages){
    var smallest=merged.Where(page=>BaseTheme(page.Theme)!="要闻").OrderBy(Estimate).FirstOrDefault()??merged.OrderBy(Estimate).First();
    var target=merged.FirstOrDefault(page=>page!=smallest&&BaseTheme(page.Theme)==BaseTheme(smallest.Theme))??merged.FirstOrDefault(page=>page!=smallest&&BaseTheme(page.Theme)=="要闻")??merged.FirstOrDefault(page=>page!=smallest);
    if(target==null)break;
    target.Articles.AddRange(smallest.Articles);merged.Remove(smallest);merged=OrderPages(merged);
   }
   var commentaryArticles=OrderCommentary(DistinctCommentary(commentary));
   if(commentaryArticles.Count>0){
    var commentaryPages=new List<NewspaperPage>();var buffer=new List<Article>();int size=0;
    void Flush(){
     if(buffer.Count==0)return;
     commentaryPages.Add(new NewspaperPage{Theme=ThemeWithContinuation("评论",commentaryPages.Count),Headline=commentaryPages.Count==0?(buffer[0].Title??"评论"):(string.IsNullOrWhiteSpace(buffer[0].Title)?"评论":buffer[0].Title),Deck=commentaryPages.Count==0?commentaryDeck:"",Articles=new List<Article>(buffer)});
     buffer.Clear();size=0;
    }
    foreach(var article in commentaryArticles){int weight=Estimate(article);if(buffer.Count>0&&size+weight>PaperLayout.CommentaryCapacity)Flush();buffer.Add(article);size+=weight;}
    Flush();
    foreach(var page in commentaryPages)merged.Add(page);
    result.CommentaryPage=commentaryPages[0];result.HasCommentary=true;
   }
   paper.Pages=merged;paper.Articles=merged.SelectMany(page=>page.Articles).ToList();result.Pages=merged;
   return result;
  }
 }
}
