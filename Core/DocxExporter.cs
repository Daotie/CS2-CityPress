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
     var groups=PaperLayout.Group(page.Articles??new System.Collections.Generic.List<Article>());
     var original=table.Element(w+"tr").Elements(w+"tc").ToArray();
     var width=table.Element(w+"tblGrid").Elements(w+"gridCol").Sum(e=>(int)e.Attribute(w+"w"));
     var grid=new XElement(w+"tblGrid");var row=new XElement(w+"tr");
     for(int i=0;i<groups.Count;i++) {
      int columnWidth=(int)Math.Round((double)width/groups.Count);grid.Add(new XElement(w+"gridCol",new XAttribute(w+"w",columnWidth)));
      var properties=new XElement(original[Math.Min(i,original.Length-1)].Element(w+"tcPr"));
      properties.Element(w+"tcW")?.SetAttributeValue(w+"w",columnWidth);
      var cell=new XElement(w+"tc",properties);
      foreach(var article in groups[i]) {
       cell.Add(P((article.Section??"")+(article.Genre=="评论"||article.Genre=="分析"?" · "+article.Genre:""),"CPSection"),P(article.Title,"Heading2"));
       foreach(var line in (article.Body??"").Split(new[]{'\n'},StringSplitOptions.RemoveEmptyEntries))cell.Add(P(line,"Normal"));
      }
      if(!groups[i].Any())cell.Add(P("","Normal"));row.Add(cell);
     }
     table.Element(w+"tblGrid").ReplaceWith(grid);table.Element(w+"tr").ReplaceWith(row);
     string date=paper.Day;
     if(DateTime.TryParseExact(paper.Day,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var issueDate))date=issueDate.ToString("yyyy年MM月dd日",CultureInfo.InvariantCulture)+" 星期"+"日一二三四五六"[(int)issueDate.DayOfWeek];
     foreach(var text in pageBody.Descendants(w+"t")){
      if(text.Value=="综合")text.Value=page.Theme??"综合";
      else if(text.Value=="综合版 · 01")text.Value=(page.Theme??"综合")+"版 · "+(pageIndex+1).ToString("D2");
      else if(text.Value=="综合 · 城市新闻")text.Value=(page.Theme??"综合")+" · "+(pageIndex+1).ToString("D2");
      text.Value=text.Value.Replace("{{报纸名称}}",paper.Name??"").Replace("{{刊发日期}}",date).Replace("{{本期头条}}",page.Headline??paper.Headline??"").Replace("{{导语}}",page.Deck??paper.Deck??"");
     }
     if(pageIndex>0)body.Add(new XElement(w+"p",new XElement(w+"r",new XElement(w+"br",new XAttribute(w+"type","page")))));
     body.Add(pageBody.Elements().ToArray());
     }
     if(templateBody.Element(w+"sectPr")!=null)body.Add(new XElement(templateBody.Element(w+"sectPr")));
     entry.Delete();using(var stream=zip.CreateEntry("word/document.xml").Open())doc.Save(stream);
    }
    if(File.Exists(path))File.Replace(tmp,path,null);else File.Move(tmp,path);
   }finally {if(File.Exists(tmp))File.Delete(tmp);}
  }
 }
}
