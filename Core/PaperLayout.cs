using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace CityPress.Core {
 public static class PaperLayout {
  public static List<NewspaperPage> Pages(Newspaper paper)=>paper.Pages!=null&&paper.Pages.Count>0?paper.Pages:new List<NewspaperPage>{new NewspaperPage{Theme="综合",Headline=paper.Headline,Deck=paper.Deck,Articles=paper.Articles??new List<Article>()}};
  static readonly JObject spec=Load();
  static JObject Load(){using(var stream=typeof(PaperLayout).Assembly.GetManifestResourceStream("CityPress.paper-layout.json"))using(var reader=new StreamReader(stream))return JObject.Parse(reader.ReadToEnd());}
  public static int Columns(int count)=>Math.Max(1,Math.Min(count,count<3?(int)spec["sparseColumns"]:(int)spec["maxColumns"]));
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
