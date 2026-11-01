using System;
using System.Linq;
namespace CityPress.Core {
 public static class EditionRules {
  static readonly string[] processPhrases={"施工标记","仍待观察","实际服务情况待观察","不代表完整自然日","观测区间","不等同于","制作过程","原生分组","采集口径","以游戏为准","游戏内","玩家操作","模组","存档","原生单位","实体编号","报道未获知"};
  public static void Normalize(Newspaper paper) {
   if(paper==null||string.IsNullOrWhiteSpace(paper.Headline)||paper.Headline.Length>300)throw new InvalidOperationException("返回的报纸标题无效，未存入缓存。");
   if(paper.Pages!=null&&paper.Pages.Count>0){
    if(paper.Pages.Count>8||paper.Pages.Any(p=>p==null||string.IsNullOrWhiteSpace(p.Theme)||p.Theme.Length>24||p.Articles==null||p.Articles.Count==0||string.IsNullOrWhiteSpace(p.Headline)))throw new InvalidOperationException("返回的分版结构无效，未存入缓存。");
    paper.Articles=paper.Pages.SelectMany(p=>p.Articles).ToList();
   }
   if(paper.Articles==null||paper.Articles.Count>24||paper.Articles.Any(a=>a==null||string.IsNullOrWhiteSpace(a.Title)||string.IsNullOrWhiteSpace(a.Body)||a.Title.Length>300||a.Body.Length>12000)||paper.Articles.Sum(a=>a.Body.Length)>80000)throw new InvalidOperationException("返回的报纸正文无效，未存入缓存。");
   foreach(var article in paper.Articles){if(string.IsNullOrWhiteSpace(article.Genre))article.Genre="消息";if(!new[]{"消息","简讯","评论","分析"}.Contains(article.Genre))throw new InvalidOperationException("返回的文章体裁无效，未存入缓存。");}
   var copy=string.Join("\n",PaperLayout.Pages(paper).Select(p=>p.Theme+"\n"+p.Headline+"\n"+p.Deck))+"\n"+paper.Headline+"\n"+paper.Deck+"\n"+string.Join("\n",paper.Articles.Select(a=>a.Title+"\n"+a.Body));
   if(processPhrases.Any(copy.Contains))throw new InvalidOperationException("稿件含制作口径或模板式免责措辞，未缓存；请重新编印。");
  }
 }
}
