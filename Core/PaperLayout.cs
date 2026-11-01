using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace CityPress.Core {
 public static class PaperLayout {
  public static List<NewspaperPage> Pages(Newspaper paper)=>paper.Pages!=null&&paper.Pages.Count>0?paper.Pages:new List<NewspaperPage>{new NewspaperPage{Theme="综合",Headline=paper.Headline,Deck=paper.Deck,Articles=paper.Articles??new List<Article>()}};
  static readonly JObject spec=Load();
  static JObject Load(){using(var stream=typeof(PaperLayout).Assembly.GetManifestResourceStream("CityPress.paper-layout.json"))using(var reader=new StreamReader(stream))return JObject.Parse(reader.ReadToEnd());}
  public static int Columns(int count)=>Math.Max(1,Math.Min(count,count<3?(int)spec["sparseColumns"]:(int)spec["maxColumns"]));
  public static string[] Paragraphs(string text){
   var lines=(text??"").Split('\n').Select(x=>x.Trim()).Where(x=>x.Length>0).ToArray();if(lines.Length!=1||lines[0].Length<=450)return lines;
   var result=new List<string>();string current="";foreach(var sentence in Regex.Split(lines[0],@"(?<=[。！？])")){current+=sentence;if(current.Length>=220){result.Add(current);current="";}}if(current.Length>0)result.Add(current);return result.ToArray();
  }
  public static string BlockText(NewspaperPage page,LayoutBlock block){
   if(block.Kind=="headline"||block.Kind=="vertical-headline")return page.Headline??"";if(block.Kind=="deck")return page.Deck??"";
   if(block.Article<0||block.Article>=page.Articles.Count)return "";var article=page.Articles[block.Article];
   if(block.Kind=="section")return (article.Section??page.Theme)+(article.Genre=="评论"||article.Genre=="分析"?" · "+article.Genre:"");if(block.Kind=="title")return article.Title??"";
   var paragraphs=Paragraphs(article.Body).Skip(block.Start);return string.Join("\n",block.Count<0?paragraphs:paragraphs.Take(block.Count));
  }
  static LayoutBlock B(string kind,int article=-1,int start=0,int count=-1)=>new LayoutBlock{Kind=kind,Article=article,Start=start,Count=count};
  static IEnumerable<LayoutBlock> Story(NewspaperPage page,int index,bool omitTitle=false){yield return B("section",index);if(!omitTitle)yield return B("title",index);yield return B("body",index);}
  public static void Assign(Newspaper paper,string mode){
   var pages=Pages(paper);paper.Pages=pages;
   foreach(var page in pages){
    var articles=page.Articles;int total=articles.Sum(a=>(a.Body??"").Length);bool opinion=articles.Count>0&&articles.All(a=>a.Genre=="评论"||(a.Genre=="分析"&&(page.Theme??"").Contains("评论")));
    uint seed=Convert.ToUInt32(Storage.Hash((paper.Name??"")+paper.Day+page.Theme+page.Headline+string.Join("\n",articles.Select(a=>a.Title+"\n"+a.Body))).Substring(0,8),16);
    string template="banner";bool vertical=(page.Headline??"").Length>0&&(page.Headline??"").Length<=(int)spec["templates"]["vertical-lead"]["maxHeadlineLength"];
    if(opinion&&total>=350)template="opinion";
    else if(mode=="varied") {var candidates=new List<string>{"banner"};if(articles.Count>=2)candidates.Add("lead-left");if(vertical&&articles.Count>0)candidates.Add("vertical-lead");if(articles.Count>=4)candidates.Add("digest");template=candidates[(int)(seed%candidates.Count)];}
    else if(articles.Count>=5&&articles.All(a=>(a.Body??"").Length<650))template="digest";
    else if(articles.Count>=2&&(articles[0].Body??"").Length>=total*.4)template="lead-left";
    else if(vertical&&articles.Count>0&&articles.Count<=3&&seed%3==1)template="vertical-lead";
    page.Composition=Compose(page,template);
   }
  }
  public static PageComposition Compose(NewspaperPage page,string template){
   if(spec["templates"][template]==null)template="banner";
   var plan=new PageComposition{Template=template};var articles=page.Articles;
   if(template=="opinion"){
    var blocks=new List<LayoutBlock>{B("headline"),B("deck")};
    for(int i=0;i<articles.Count;i++){blocks.AddRange(Story(page,i,i==0&&articles[i].Title==page.Headline).Where(b=>b.Kind!="body"));for(int paragraph=0;paragraph<Paragraphs(articles[i].Body).Length;paragraph++)blocks.Add(B("body",i,paragraph,1));}
    int count=Math.Min(articles.Sum(a=>Paragraphs(a.Body).Length),articles.Sum(a=>(a.Body??"").Length)>=(int)spec["templates"]["opinion"]["threeColumnsAt"]?3:2);count=Math.Max(1,count);
    int cursor=0;double remaining=blocks.Sum(b=>Weight(page,b));
    for(int c=0;c<count;c++){var column=new LayoutColumn{Width=1};double used=0,target=remaining/(count-c);while(cursor<blocks.Count){var block=blocks[cursor++];column.Blocks.Add(block);used+=Weight(page,block);if(block.Kind=="body"&&c<count-1&&blocks.Skip(cursor).Count(b=>b.Kind=="body")>=count-c-1&&(used>=target||(cursor<blocks.Count&&Math.Abs(used-target)<=Math.Abs(used+Weight(page,blocks[cursor])-target))))break;}remaining-=used;plan.Columns.Add(column);}return plan;
   }
   if(template=="lead-left"&&articles.Count>=2){
    var lead=new LayoutColumn{Width=(int)spec["templates"][template]["leadWidth"]};lead.Blocks.AddRange(new[]{B("headline"),B("deck")});lead.Blocks.AddRange(Story(page,0,articles[0].Title==page.Headline));var side=new LayoutColumn{Width=100-lead.Width};for(int i=1;i<articles.Count;i++)side.Blocks.AddRange(Story(page,i));plan.Columns.Add(lead);plan.Columns.Add(side);return plan;
   }
   if(template=="vertical-lead"&&articles.Count>0&&(page.Headline??"").Length<=(int)spec["templates"][template]["maxHeadlineLength"]){
    int width=(int)spec["templates"][template]["leadWidth"];plan.Columns.Add(new LayoutColumn{Width=width,Blocks=new List<LayoutBlock>{B("vertical-headline")}});var body=new LayoutColumn{Width=100-width};body.Blocks.Add(B("deck"));for(int i=0;i<articles.Count;i++)body.Blocks.AddRange(Story(page,i,i==0&&articles[i].Title==page.Headline));plan.Columns.Add(body);return plan;
   }
   if(template!="digest") {plan.Template="banner";plan.Top.AddRange(new[]{B("headline"),B("deck")});}
   int index=0;foreach(var group in Group(articles)){var column=new LayoutColumn{Width=1};if(template=="digest"&&index==0)column.Blocks.AddRange(new[]{B("headline"),B("deck")});foreach(var article in group){column.Blocks.AddRange(Story(page,index,template=="digest"&&index==0&&article.Title==page.Headline));index++;}plan.Columns.Add(column);}return plan;
  }
  static double Weight(NewspaperPage page,LayoutBlock block)=>BlockText(page,block).Length*(block.Kind=="headline"?3:block.Kind=="title"?2:1)+(block.Kind=="body"?12:25);
  public static PageComposition Composition(NewspaperPage page)=>page.Composition??Compose(page,"banner");
  public static List<string[]> VerticalColumns(string text){var glyphs=new List<string>();for(int i=0;i<(text??"").Length;i++){int size=char.IsHighSurrogate(text[i])&&i+1<text.Length&&char.IsLowSurrogate(text[i+1])?2:1;glyphs.Add(text.Substring(i,size));i+=size-1;}int count=(int)spec["templates"]["vertical-lead"]["glyphsPerColumn"];return Enumerable.Range(0,(glyphs.Count+count-1)/count).Select(i=>glyphs.Skip(i*count).Take(count).ToArray()).ToList();}
  public static List<List<Article>> Group(IReadOnlyList<Article> articles) {
   var count=Columns(articles.Count);var groups=new List<List<Article>>();
   var weights=articles.Select(a=>(a.Body??"").Length+(a.Title??"").Length*(int)spec["titleWeight"]+(int)spec["articleOverhead"]).ToArray();
   double remaining=weights.Sum();int cursor=0;
   for(int column=0;column<count;column++){
    var group=new List<Article>();var target=remaining/(count-column);double used=0;
    while(cursor<articles.Count){
     if(group.Count>0 && (articles.Count-cursor<=count-column-1 || Math.Abs(used-target)<=Math.Abs(used+weights[cursor]-target)))break;
     used+=weights[cursor];group.Add(articles[cursor++]);
    }
    remaining-=used;groups.Add(group);
   }
   return groups;
  }
 }
}
