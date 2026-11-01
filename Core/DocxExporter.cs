using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
namespace CityPress.Core {
 public static class DocxExporter {
  public static string TemplatePath {get;set;}
  static readonly XNamespace w="http://schemas.openxmlformats.org/wordprocessingml/2006/main";
  static XElement P(string text,string style)=>new XElement(w+"p",new XElement(w+"pPr",new XElement(w+"pStyle",new XAttribute(w+"val",style))),new XElement(w+"r",new XElement(w+"t",text??"")));
  static XElement Vertical(string text){var paragraph=P(text,"Heading2");paragraph.Element(w+"r").AddFirst(new XElement(w+"rPr",new XElement(w+"sz",new XAttribute(w+"val",30))));return paragraph;}
  // Long commentary subheads reuse the article-heading style with a smaller
  // run so the Word template's Chinese font is preserved.
  static XElement Subhead(string text){
   var paragraph=P(text,"Heading2");
   paragraph.Element(w+"r").AddFirst(new XElement(w+"rPr",new XElement(w+"b"),new XElement(w+"sz",new XAttribute(w+"val",27))));
   var properties=paragraph.Element(w+"pPr");
   if(properties!=null){
    var style=properties.Element(w+"pStyle");
    var keep=new XElement(w+"keepNext");
    if(style==null)properties.AddFirst(keep);else{var elements=properties.Elements().ToList();var position=elements.IndexOf(style)+1;if(position>=elements.Count)properties.Add(keep);else elements[position].AddBeforeSelf(keep);}
   }
   return paragraph;
  }
  static readonly System.Collections.Generic.Dictionary<string,string> ThemeEnglish=new System.Collections.Generic.Dictionary<string,string>{
   {"综合","General"},{"要闻","Front Page"},{"经济与财政","Economy & Finance"},{"城市建设与交通","Construction & Transport"},
   {"民生与社会","Livelihood & Society"},{"环境与观察","Environment & Observation"},{"评论","Opinion"},{"城市要闻","City News"}
  };
  static readonly System.Text.RegularExpressions.Regex Continuation=new System.Text.RegularExpressions.Regex(@"（续(?:[一二三四五六七八九])?）$");
  static string ThemeLabel(string theme,bool english){
   if(!english||string.IsNullOrWhiteSpace(theme))return theme??"综合";
   var value=theme.Trim();int count=0;
   while(Continuation.IsMatch(value)){count++;value=Continuation.Replace(value,"").Trim();}
   var label=ThemeEnglish.TryGetValue(value,out var translated)?translated:value;
   for(int i=0;i<count;i++)label+=" (cont.)";
   return label;
  }
  static bool IsEnglish(Newspaper paper){
   var articles=paper.Articles??PaperLayout.Pages(paper).SelectMany(page=>page.Articles).ToList();
   var sample=string.Join("\n",new[]{paper.Headline??"",paper.Deck??""}.Concat(articles.Select(article=>(article?.Title??"")+"\n"+(article?.Body??""))));
   return !I18n.MostlyChinese(sample);
  }
  static string WordDate(string day,bool english){
   if(!DateTime.TryParseExact(day,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var issueDate))return day;
   if(english){
    var months=new[]{"January","February","March","April","May","June","July","August","September","October","November","December"};
    var weekdays=new[]{"Sunday","Monday","Tuesday","Wednesday","Thursday","Friday","Saturday"};
    return months[issueDate.Month-1]+" "+issueDate.Day+", "+issueDate.Year+" · "+weekdays[(int)issueDate.DayOfWeek];
   }
   return issueDate.ToString("yyyy年MM月dd日",CultureInfo.InvariantCulture)+" 星期"+"日一二三四五六"[(int)issueDate.DayOfWeek];
  }
  static string TemplateText(string value,string theme,bool english,int pageIndex){
   if(value=="综合")return theme;
   if(value=="综合版 · 01")return english?theme+" · PAGE "+(pageIndex+1).ToString("D2"):theme+"版 · "+(pageIndex+1).ToString("D2");
   if(value=="综合 · 城市新闻")return english?theme+" · CITY NEWS":theme+" · "+(pageIndex+1).ToString("D2");
   if(english){
    if(value=="城市新闻")return "CITY NEWS";
    if(value=="城市纪事")return "CITY CHRONICLE";
    if(value=="日报")return "DAILY";
    if(value=="电子刊")return "E-PAPER";
   }
   return value;
  }
  public static void Save(string path,Newspaper paper) {
   Directory.CreateDirectory(Path.GetDirectoryName(path));var tmp=path+"."+Guid.NewGuid().ToString("N")+".tmp";
   try {
    using(var template=!string.IsNullOrEmpty(TemplatePath)&&File.Exists(TemplatePath)?File.OpenRead(TemplatePath):typeof(DocxExporter).Assembly.GetManifestResourceStream("CityPress.Templates.newspaper.docx")) {
     if(template==null)throw new InvalidOperationException("报纸模板不可用，请重新安装完整的 CityPress。");
     using(var output=File.Create(tmp))template.CopyTo(output);
    }
    using(var zip=ZipFile.Open(tmp,ZipArchiveMode.Update)) {
     var entry=zip.GetEntry("word/document.xml");XDocument doc;using(var stream=entry.Open())doc=XDocument.Load(stream);
     var body=doc.Root.Element(w+"body");
     var templateBody=new XElement(body);body.RemoveNodes();var pages=PaperLayout.Pages(paper);
     for(int pageIndex=0;pageIndex<pages.Count;pageIndex++){
     var page=pages[pageIndex];var pageBody=new XElement(templateBody);pageBody.Element(w+"sectPr")?.Remove();
     var table=pageBody.Elements(w+"tbl").FirstOrDefault(t=>(string)t.Element(w+"tblPr")?.Element(w+"tblCaption")?.Attribute(w+"val")=="CityPress.Articles");
     if(table==null)throw new InvalidOperationException("Word 模板版本过旧，请使用随本版安装的统一报纸模板。");
     var plan=PaperLayout.Composition(page);
     if(plan.Top.Count==0)foreach(var paragraph in pageBody.Elements(w+"p").Where(p=>p.Descendants(w+"t").Any(t=>t.Value.Contains("{{本期头条}}")||t.Value.Contains("{{导语}}"))).ToArray())paragraph.Remove();
     var original=table.Element(w+"tr").Elements(w+"tc").ToArray();
     var width=table.Element(w+"tblGrid").Elements(w+"gridCol").Sum(e=>(int)e.Attribute(w+"w"));
     var grid=new XElement(w+"tblGrid");var row=new XElement(w+"tr");
     for(int i=0;i<plan.Columns.Count;i++) {
      var column=plan.Columns[i];int columnWidth=(int)Math.Round((double)width*column.Width/plan.Columns.Sum(c=>c.Width));grid.Add(new XElement(w+"gridCol",new XAttribute(w+"w",columnWidth)));
      var properties=new XElement(original[Math.Min(i,original.Length-1)].Element(w+"tcPr"));
      properties.Element(w+"tcW")?.SetAttributeValue(w+"w",columnWidth);
      var cell=new XElement(w+"tc",properties);
      foreach(var block in column.Blocks){
       var text=PaperLayout.BlockText(page,block);if(string.IsNullOrWhiteSpace(text))continue;
       if(block.Kind=="vertical-headline"){properties.Add(new XElement(w+"textDirection",new XAttribute(w+"val","tbRl")));cell.Add(Vertical(text));continue;}
       if(block.Kind=="subhead"){cell.Add(Subhead(text));continue;}
       foreach(var line in text.Split(new[]{'\n'},StringSplitOptions.RemoveEmptyEntries)){
        if(PaperLayout.IsSubhead(line)){cell.Add(Subhead(PaperLayout.SubheadText(line)));continue;}
        var paragraph=P(line,block.Kind=="section"?"CPSection":block.Kind=="headline"||block.Kind=="title"?"Heading2":block.Kind=="deck"?"CPDeck":"Normal");
        if(block.Kind=="headline")paragraph.Element(w+"r").AddFirst(new XElement(w+"rPr",new XElement(w+"sz",new XAttribute(w+"val",plan.Template=="digest"?30:38))));cell.Add(paragraph);
       }
      }
      if(cell.Elements().LastOrDefault()?.Name!=w+"p")cell.Add(P("","Normal"));row.Add(cell);
     }
     table.Element(w+"tblGrid").ReplaceWith(grid);table.Element(w+"tr").ReplaceWith(row);
     bool english=IsEnglish(paper);string date=WordDate(paper.Day,english);
     foreach(var text in pageBody.Descendants(w+"t")){
      text.Value=TemplateText(text.Value,ThemeLabel(page.Theme??"综合",english),english,pageIndex);
      text.Value=text.Value.Replace("{{报纸名称}}",paper.Name??"").Replace("{{刊发日期}}",date).Replace("{{本期头条}}",page.Headline??paper.Headline??"").Replace("{{导语}}",page.Deck??paper.Deck??"");
     }
     if(pageIndex>0)body.Add(new XElement(w+"p",new XElement(w+"r",new XElement(w+"br",new XAttribute(w+"type","page")))));
     body.Add(pageBody.Elements().ToArray());
     }
     if(templateBody.Element(w+"sectPr")!=null)body.Add(new XElement(templateBody.Element(w+"sectPr")));
     entry.Delete();using(var stream=zip.CreateEntry("word/document.xml").Open())doc.Save(stream);
    }
    Storage.Commit(tmp,path);
   }finally {if(File.Exists(tmp))File.Delete(tmp);}
  }
 }
}
