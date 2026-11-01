using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace CityPress.Core {
 public sealed class QualityReview {
  public double Score=100;
  public List<QualityIssue> Issues=new List<QualityIssue>();
  public bool HasFatal=>Issues.Any(issue=>issue.Severity=="fatal");
  public bool NeedsRevision=>Issues.Any(issue=>issue.Severity=="fatal"||issue.Severity=="major");
  public bool CommentaryCopy=>Issues.Any(issue=>issue.Code=="commentary-copy");
  public string Summary=>string.Join("；",Issues.Where(issue=>issue.Severity!="minor").Take(4).Select(issue=>issue.Detail));
 }
 // Step 6 of the editorial process: deterministic quality control before layout.
 // Checks run on the finished copy; a single targeted revision may follow, then
 // unsafe copy is removed rather than published with invented facts.
 public static class QualityControl {
  const string Fatal="fatal",Major="major",Minor="minor";
  static readonly Regex Attribution=new Regex(@"记者|通讯员|采访|受访|新闻发言人|发布会|有关部门|相关部门|据[^，。；\n]{0,6}(介绍|表示|透露|通报)|官方(表示|通报|消息)|市政府(表示|通报)|统计局|调查(显示|结果)|民调|市民(表示|认为|反映)|居民(表示|认为)|专家(表示|指出|认为)|学者(表示|指出)|业内人士|知情人士",RegexOptions.CultureInvariant);
  static readonly Regex Mechanical=new Regex(@"意义重大|成效显著|稳步推进|值得关注|再上新台阶|迈上新台阶|谱写|新篇章|持续向好|成效初显|有力推动|极大提升|开创新局面|注入新动力",RegexOptions.CultureInvariant);
  static readonly Regex GenericTitle=new Regex(@"取得新进展|再上新台阶|迈上新台阶|成效显著|意义重大|稳步推进|持续向好|迎来新变化|谱写|新篇章|又有新突破|展现新面貌|发展取得|不断取得",RegexOptions.CultureInvariant);
  static readonly Regex GenericOpening=new Regex(@"^(观测区间内|当日观测|数据显示|从当前记录来看|据了解|相关状态|尚未体现|值得注意的是|随着)",RegexOptions.CultureInvariant);
  static readonly Regex AnalysisMarker=new Regex(@"意味着|说明|原因|背景|影响|挑战|选择|趋势|问题|风险|取决于|关键|长期|短期|衡量|权衡|代价|可持续|公共服务|公共利益",RegexOptions.CultureInvariant);
  static readonly Regex DateLike=new Regex(@"\d{4}\s*年\s*\d{1,2}\s*月\s*\d{1,2}\s*日|\d{4}-\d{1,2}-\d{1,2}|\d{1,2}:\d{2}(?::\d{2})?|第\s*\d+\s*版|\d{1,2}\s*[时点]\s*\d{1,2}\s*分?|\d{1,2}\s*月\s*\d{1,2}\s*日",RegexOptions.CultureInvariant);
  static readonly Regex NumberToken=new Regex(@"(\d+(?:\.\d+)?)\s*(万|亿)?\s*(%|％)?",RegexOptions.CultureInvariant);
  sealed class FactBook {
   public readonly List<double> Values=new List<double>();
   public readonly List<double> Percents=new List<double>();
   public readonly List<string> Labels=new List<string>();
   public readonly List<string> Names=new List<string>();
   public int ReportableMetrics;
   public void Add(double value){if(!double.IsNaN(value)&&!double.IsInfinity(value))Values.Add(value);}
   public void AddPercent(double value){if(!double.IsNaN(value)&&!double.IsInfinity(value))Percents.Add(value);}
   public bool Traceable(double value,bool percent){
    var pool=percent?Percents:Values;
    foreach(var source in pool)if(CloseEnough(value,source))return true;
    // A money or population figure may legitimately be scaled by 万/亿 in copy.
    return false;
   }
   public static int SignificantDigits(double value){
    var text=Math.Abs(value).ToString("0.################",CultureInfo.InvariantCulture);
    if(text.Contains("."))text=text.TrimEnd('0').TrimEnd('.');
    text=text.TrimStart('0').Replace(".","").TrimStart('0').TrimEnd('0');
    return Math.Max(1,text.Length);
   }
   public static bool CloseEnough(double value,double source){
    double absolute=Math.Abs(value),baseline=Math.Abs(source);
    if(absolute<=12&&baseline<=12)return true;
    if(absolute>=1900&&absolute<=2100&&source==0)return true;
    int digits=SignificantDigits(value);
    double tolerance=digits<=2?0.05:digits==3?0.015:digits==4?0.004:0.001;
    double allowed=Math.Max(baseline*tolerance,0.000001);
    return Math.Abs(absolute-baseline)<=allowed;
   }
  }
  static FactBook BuildFacts(DayLog day) {
   var book=new FactBook();if(day==null)return book;
   foreach(var metric in day.Metrics??new List<MetricRecord>()){
    if(metric==null)continue;
    book.Add(metric.First);book.Add(metric.Last);book.Add(metric.Min);book.Add(metric.Max);
    book.Add(Math.Abs(metric.Last-metric.First));book.Add(Math.Abs(metric.Max-metric.Min));
    if(metric.First!=0)book.AddPercent(Math.Abs((metric.Last-metric.First)/metric.First)*100);
    if(metric.First!=0)book.AddPercent(Math.Abs(metric.Last/metric.First)*100);
    if(0<Math.Abs(metric.Last)&&Math.Abs(metric.Last)<1)book.AddPercent(Math.Abs(metric.Last)*100);
    if(0<Math.Abs(metric.First)&&Math.Abs(metric.First)<1)book.AddPercent(Math.Abs(metric.First)*100);
    if(!string.IsNullOrWhiteSpace(metric.Label))book.Labels.Add(metric.Label);
    if(NewsValue.IsReportableMetric(metric))book.ReportableMetrics++;
   }
   foreach(var record in day.Breakthroughs??new List<BreakthroughRecord>()){
    if(record==null)continue;book.Add(record.PreviousHigh);book.Add(record.Threshold);book.Add(record.Value);
    if(!string.IsNullOrWhiteSpace(record.Label))book.Labels.Add(record.Label);
   }
   foreach(var line in day.Transit??new List<TransitRecord>()){
    if(line==null)continue;book.Add(line.Stops);book.Add(line.Vehicles);book.Add(line.Fare);
    if(!string.IsNullOrWhiteSpace(line.Name))book.Names.Add(line.Name);
   }
   var actions=EditorialInput.Select(day);book.Add(actions.Count);book.Add(actions.Sum(action=>action.Count));
   foreach(var action in actions)book.Add(action.Count);
   return book;
  }
  static IEnumerable<Tuple<double,bool>> Numbers(string text) {
   if(string.IsNullOrWhiteSpace(text))yield break;
   var clean=DateLike.Replace(text," ");
   foreach(Match match in NumberToken.Matches(clean)){
    if(!double.TryParse(match.Groups[1].Value,NumberStyles.Float,CultureInfo.InvariantCulture,out var number))continue;
    var unit=match.Groups[2].Value;var percent=match.Groups[3].Value.Length>0;
    if(unit=="万")number*=10000;else if(unit=="亿")number*=100000000;
    yield return Tuple.Create(number,percent);
   }
  }
  static double ScoreOf(IEnumerable<QualityIssue> issues) {
   double score=100;
   foreach(var issue in issues)score-=issue.Severity==Fatal?45:issue.Severity==Major?8:3;
   if(issues.Any(issue=>issue.Severity==Fatal))score=Math.Min(score,45);
   return Math.Max(0,Math.Min(100,score));
  }
  static string Normalize(string text)=>Regex.Replace(text??"",@"[\s，。；：、！？“”‘’（）()《》\-—…,.!?;:0-9%％]","",RegexOptions.CultureInvariant);
  public static double Similarity(string left,string right) {
   var a=Normalize(left);var b=Normalize(right);
   if(a.Length<20||b.Length<20)return a.Length>0&&a==b?1:0;
   const int size=4;var setA=new HashSet<string>();var setB=new HashSet<string>();
   for(int i=0;i+size<=a.Length;i++)setA.Add(a.Substring(i,size));
   for(int i=0;i+size<=b.Length;i++)setB.Add(b.Substring(i,size));
   if(setA.Count==0||setB.Count==0)return 0;
   int overlap=setA.Count(item=>setB.Contains(item));
   return (double)overlap/(setA.Count+setB.Count-overlap);
  }
  static void Flag(Article article,List<QualityIssue> issues,string code,string severity,string detail) {
   var issue=new QualityIssue{Code=code,Severity=severity,Detail=detail};
   issues.Add(issue);article.Issues.Add(issue);
  }
  static void CheckArticle(Article article,int index,List<Article> articles,FactBook facts,EditorialBrief brief,Article commentaryLead,List<QualityIssue> issues,bool commentaryPage) {
   if(article==null)return;
   if(string.IsNullOrWhiteSpace(article.Title))Flag(article,issues,"title-missing",Fatal,"第 "+(index+1)+" 篇稿件缺少标题");
   else if(article.Title.Length>60)Flag(article,issues,"title-too-long",Major,"标题过长："+article.Title);
   else if(article.Title.Length>40)Flag(article,issues,"title-long",Minor,"标题偏长："+article.Title);
   if(GenericTitle.IsMatch(article.Title??""))Flag(article,issues,"title-generic",Major,"标题空泛："+article.Title);
   if(string.IsNullOrWhiteSpace(article.Body)){Flag(article,issues,"body-missing",Fatal,"《"+(article.Title??"无题")+"》缺少正文");return;}
   var body=article.Body;
   if(body.Length<40)Flag(article,issues,"body-short",Major,"《"+article.Title+"》正文过短，信息不足");
   // Never publish numbers that cannot be traced back to observed facts.
   if(facts.Values.Count>=8&&facts.ReportableMetrics>=2){
    foreach(var token in Numbers(body+"\n"+(article.Title??""))){
     if(token.Item2)continue; // percentages are checked softly below
     if(Math.Abs(token.Item1)<=12)continue;
     if(token.Item1>=1900&&token.Item1<=2100)continue;
     if(facts.Traceable(token.Item1,false))continue;
     Flag(article,issues,"number-unverified",commentaryPage?Major:Fatal,"《"+article.Title+"》中的数字 "+token.Item1.ToString("0.##",CultureInfo.InvariantCulture)+" 无法追溯到本期采集数据");
     break;
    }
    foreach(var token in Numbers(body).Where(token=>token.Item2)){
     if(facts.Traceable(token.Item1,true))continue;
     Flag(article,issues,"percent-unverified",Minor,"《"+article.Title+"》中的百分比 "+token.Item1.ToString("0.##",CultureInfo.InvariantCulture)+"% 缺少直接数据依据");
     break;
    }
   }
   if(EditionRules.ContainsProcessPhrase((article.Title??"")+"\n"+body))Flag(article,issues,"process-phrase",Major,"《"+article.Title+"》含制作口径或模板式免责措辞，需重写");
   if(Attribution.IsMatch(body))Flag(article,issues,"fabricated-source",Fatal,"《"+article.Title+"》出现无法核实的采访、引述或消息来源");
   if(Mechanical.IsMatch(body))Flag(article,issues,"mechanical-evaluation",Major,"《"+article.Title+"》含缺少事实支撑的评价性套话");
   if(GenericOpening.IsMatch((body??"").TrimStart()))Flag(article,issues,"mechanical-opening",Major,"《"+article.Title+"》使用了模板式开头");
   var sentences=Regex.Split(body,@"(?<=[。！？])").Where(sentence=>sentence.Trim().Length>0).ToList();
   if(sentences.Count>0&&sentences.Count(sentence=>sentence.Any(char.IsDigit))*2>sentences.Count)Flag(article,issues,"number-dump",Minor,"《"+article.Title+"》以数字罗列代替叙述");
   if(! (article.Genre=="评论") &&body.Contains("## "))Flag(article,issues,"subhead-outside-commentary",Major,"小标题只用于评论版：《"+article.Title+"》");
   if(article.Genre=="评论"){
    int commentaryLength=PaperLayout.BodyLength(body);
    var paragraphs=PaperLayout.Paragraphs(body);
    var subheads=paragraphs.Where(PaperLayout.IsSubhead).Select(PaperLayout.SubheadText).ToList();
    bool isLead=commentaryLead!=null&&ReferenceEquals(article,commentaryLead);
    bool isLong=commentaryLength>=450;
    if(commentaryLength<40)Flag(article,issues,"commentary-too-short",Fatal,"评论《"+article.Title+"》不足 40 字，未形成有效内容");
    else if(isLead&&commentaryLength<120)Flag(article,issues,"commentary-too-short",Fatal,"长评论《"+article.Title+"》不足 120 字，未形成有效论证");
    else if((isLead||isLong)&&commentaryLength<300)Flag(article,issues,"commentary-short",Major,"评论《"+article.Title+"》仅 "+commentaryLength+" 字，论证不足");
    else if(commentaryLength<120)Flag(article,issues,"short-comment-short",Major,"短评《"+article.Title+"》仅 "+commentaryLength+" 字，信息不足");
    int markers=AnalysisMarker.Matches(body).Count;
    if((isLead||isLong)&&markers<2)Flag(article,issues,"commentary-no-analysis",Major,"评论《"+article.Title+"》缺少分析论证，接近新闻复述");
    else if(!isLead&&!isLong&&markers<1)Flag(article,issues,"short-comment-no-viewpoint",Major,"短评《"+article.Title+"》没有明确观点");
    if((isLead||isLong)&&commentaryLength>=400&&!body.Contains("\n")&&subheads.Count==0)Flag(article,issues,"commentary-no-paragraphs",Major,"评论《"+article.Title+"》没有自然段划分");
    if(commentaryLength>=500){
     if(subheads.Count<2)Flag(article,issues,"commentary-no-subheads",Major,"长评论《"+article.Title+"》缺少小标题层次（正文中用单独一行“## 小标题”分隔段落）");
     else if(subheads.Count>6)Flag(article,issues,"commentary-subheads-many",Minor,"长评论《"+article.Title+"》小标题过多");
     if(subheads.Count>0&&paragraphs.Length>0&&PaperLayout.IsSubhead(paragraphs.Last()))Flag(article,issues,"commentary-subhead-tail",Major,"评论《"+article.Title+"》以小标题结尾");
     foreach(var label in subheads){
      if(label.Length<2||label.Length>20)Flag(article,issues,"commentary-subhead-length",Major,"评论《"+article.Title+"》小标题长度不合适："+label);
      else if(new[]{"背景","原因","意义","影响","分析","观点","结论","总结","展望","未来","挑战","建议","评论"}.Contains(label))Flag(article,issues,"commentary-subhead-generic",Major,"评论《"+article.Title+"》小标题空泛："+label);
     }
    }
    if(subheads.Count>0&&commentaryLength<500)Flag(article,issues,"commentary-subheads-short",Minor,"短评论《"+article.Title+"》使用小标题，版面偏碎");
    if(sentences.Count>0&&sentences.Count(sentence=>sentence.Any(char.IsDigit))*10>sentences.Count*7)Flag(article,issues,"commentary-number-dump",Major,"评论《"+article.Title+"》以数字堆砌代替观点论证");
   }
  }
  public static QualityReview Review(Newspaper paper,EditorialBrief brief,DayLog day) {
   var review=new QualityReview();if(paper==null)return review;
   var issues=review.Issues;var facts=BuildFacts(day);
   paper.Articles=paper.Articles??new List<Article>();
   foreach(var article in paper.Articles)article?.Issues.Clear();
   paper.Issues.Clear();
   if(brief!=null)foreach(var article in paper.Articles.Where(article=>article!=null)){
    foreach(var item in brief.Events.Take(40)){
     var token=string.IsNullOrWhiteSpace(item.MajorToken)?item.Subject:item.MajorToken;
     if(string.IsNullOrWhiteSpace(token)||token.Length<3)continue;
     if(PagePlanner.Text(article).Contains(token)&&!article.Sources.Contains(item.Id))article.Sources.Add(item.Id);
    }
    while(article.Sources.Count>8)article.Sources.RemoveAt(article.Sources.Count-1);
   }
   var pages=PagePlanner.PagesOf(paper);
   var comments=paper.Articles.Where(article=>article!=null&&article.Genre=="评论").ToList();
   var commentaryLead=comments.OrderByDescending(article=>PaperLayout.BodyLength(article.Body)).FirstOrDefault();
   // Content checks.
   for(int i=0;i<paper.Articles.Count;i++)CheckArticle(paper.Articles[i],i,paper.Articles,facts,brief,commentaryLead,issues,paper.Articles[i]?.Genre=="评论");
   // Repeated opening / duplicate editions.
   var openings=paper.Articles.Where(article=>article!=null&&!string.IsNullOrWhiteSpace(article.Body))
    .GroupBy(article=>new string(((article.Body??"").TrimStart().Take(12)).ToArray()).Trim())
    .Where(group=>group.Key.Length>=4&&group.Count()>=2);
   foreach(var group in openings)foreach(var article in group)Flag(article,issues,"opening-repeated",Major,"《"+article.Title+"》与另一篇稿件开头雷同");
   var titles=paper.Articles.Where(article=>article!=null).GroupBy(article=>(article.Title??"").Trim()).Where(group=>group.Key.Length>0&&group.Count()>=2);
   foreach(var group in titles)foreach(var article in group.Skip(1))Flag(article,issues,"title-duplicate",Major,"标题重复："+group.Key);
   for(int i=0;i<paper.Articles.Count;i++)for(int j=i+1;j<paper.Articles.Count;j++){
    var left=paper.Articles[i];var right=paper.Articles[j];if(left==null||right==null)continue;
    double similarity=Similarity(left.Body,right.Body);
    bool leftComment=left.Genre=="评论",rightComment=right.Genre=="评论";
    if(leftComment!=rightComment){
     if(similarity>=0.45)Flag(rightComment?right:left,issues,"commentary-copy",Fatal,"评论与新闻《"+(rightComment?left:right).Title+"》大段重复，未形成独立分析");
     continue;
    }
    if(leftComment&&rightComment){if(similarity>=0.6)Flag(right,issues,"commentary-duplicate",Major,"两篇评论内容高度重复");continue;}
    if(similarity>=0.8)Flag(right,issues,"duplicate-news",Major,"《"+right.Title+"》与《"+left.Title+"》重复报道同一内容");
   }
   // Edition-level checks.
   if(pages.Count>1&&pages.Any(page=>PagePlanner.IsGenericTheme(page.Theme)))Flag(paper.Articles.FirstOrDefault()??new Article(),issues,"page-generic",Minor,"多版报纸仍有版面只标注“综合”");
   foreach(var page in pages){
    if(page.Articles.Count==0)Flag(paper.Articles.FirstOrDefault()??new Article(),issues,"page-empty",Major,"版面《"+(page.Theme??"未命名")+"》为空");
    if(page.Articles.Count>6)Flag(page.Articles[0],issues,"page-too-many",Minor,"版面《"+page.Theme+"》文章过多，影响阅读");
    if(page.Theme!="评论"&&page.Theme!="要闻"&&!PagePlanner.IsGenericTheme(page.Theme)){
     var baseTheme=PagePlanner.BaseTheme(page.Theme);
     int mismatch=page.Articles.Where(article=>article.Genre!="评论").Count(article=>PagePlanner.ThemeOf(article)!=PagePlanner.FallbackTheme&&PagePlanner.ThemeOf(article)!=baseTheme);
     if(mismatch>0&&mismatch*2>page.Articles.Count)Flag(page.Articles[0],issues,"page-mismatch",Minor,"版面《"+page.Theme+"》与多数稿件主题不一致");
    }
   }
   var needCommentary=brief!=null&&brief.CommentaryWarranted;
   bool hasCommentary=pages.Any(page=>page.Articles.Count>0&&page.Articles.All(PagePlanner.IsCommentary));
   bool hasLongCommentary=comments.Any(article=>PaperLayout.BodyLength(article.Body)>=300);
   if(needCommentary&&!hasCommentary)Flag(paper.Articles.FirstOrDefault()??new Article(),issues,"commentary-missing",Major,"重大事件缺少独立评论版");
   else if(needCommentary&&!hasLongCommentary)Flag(paper.Articles.FirstOrDefault()??new Article(),issues,"commentary-long-missing",Major,"独立评论版缺少占据主要篇幅的长评论");
   // Milestone coverage: the top event should be visible in the news copy.
   if(brief!=null)foreach(var majorEvent in brief.MajorEvents.Where(item=>item.Importance>=NewsImportance.Milestone)){
    if(!paper.Articles.Any(article=>article!=null&&!string.IsNullOrEmpty(majorEvent.MajorToken)&&PagePlanner.Text(article).Contains(majorEvent.MajorToken)))
     Flag(paper.Articles.FirstOrDefault()??new Article(),issues,"major-uncovered",Major,"里程碑事件“"+(majorEvent.Subject??majorEvent.MajorToken)+"”未在稿件中体现");
   }
   review.Score=ScoreOf(issues);
   foreach(var article in paper.Articles)if(article!=null)article.Score=ScoreOf(article.Issues);
   return review;
  }
  public static void Attach(Newspaper paper,QualityReview review) {
   if(paper==null||review==null)return;
   paper.Issues=review.Issues.Take(40).Select(issue=>new QualityIssue{Code=issue.Code,Severity=issue.Severity,Detail=issue.Detail}).ToList();
   paper.QualityScore=(int)Math.Round(review.Score);
   paper.QualityNote=review.HasFatal?"质量检查未通过":review.NeedsRevision?"质量检查发现问题，已尝试局部修订":review.Issues.Count>0?"质量检查通过，有轻微提示":"质量检查通过";
   foreach(var article in paper.Articles??new List<Article>()){
    if(article==null)continue;
    if(string.IsNullOrWhiteSpace(article.Id))article.Id=Storage.Hash((article.Title??"")+"\n"+(article.Body??"")).Substring(0,10);
    article.Score=ScoreOf(article.Issues);
    article.Status=article.Issues.Any(issue=>issue.Severity==Fatal)?"held":article.Issues.Any(issue=>issue.Severity==Major)?"revised":"passed";
   }
  }
  // A comment that merely reprints the news is the P0 failure mode: never keep it.
  public static bool RemoveFailedCommentary(Newspaper paper,QualityReview review) {
   if(paper==null||review==null)return false;
   var failed=paper.Articles.Where(article=>article.Genre=="评论"&&article.Issues.Any(issue=>issue.Severity==Fatal||issue.Code=="commentary-copy")).ToList();
   if(failed.Count==0)return false;
   PagePlanner.RemoveCommentary(paper);
   foreach(var article in paper.Articles)article.Issues.RemoveAll(issue=>issue.Code=="commentary-copy");
   review.Issues.RemoveAll(issue=>issue.Code=="commentary-copy"||issue.Code=="commentary-too-short");
   return true;
  }
  // Removes copy that still contains invented facts after revision. Returns
  // false when nothing publishable is left, so the caller reports a failure
  // instead of silently publishing a fabricated story.
  public static bool ApplySafeDegradation(Newspaper paper,QualityReview review) {
   if(paper==null)return false;
   var dropped=new HashSet<Article>();
   foreach(var article in paper.Articles??new List<Article>()){
    if(article==null)continue;
    var fatal=article.Issues.Any(issue=>issue.Severity==Fatal);
    var duplicate=article.Issues.Any(issue=>issue.Code=="duplicate-news"||issue.Code=="commentary-duplicate");
    if(fatal||duplicate)dropped.Add(article);
   }
   if(dropped.Count>0){
    paper.DroppedUnsafeArticles+=dropped.Count;
    paper.Articles=paper.Articles.Where(article=>article!=null&&!dropped.Contains(article)).ToList();
    var pages=PagePlanner.PagesOf(paper);
    foreach(var page in pages)page.Articles=page.Articles.Where(article=>!dropped.Contains(article)).ToList();
    paper.Pages=pages.Where(page=>page.Articles.Count>0).ToList();
    if(paper.Pages.Count==0)paper.Pages=new List<NewspaperPage>();
    paper.Articles=paper.Pages.SelectMany(page=>page.Articles).ToList();
    review.Issues.RemoveAll(issue=>issue.Code=="commentary-missing"||issue.Code=="major-uncovered");
   }
   return paper.Articles.Count(article=>article.Genre!="评论")>0||paper.Articles.Any(article=>article.Genre=="评论");
  }
 }
}
