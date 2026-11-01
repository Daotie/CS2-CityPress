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
       foreach(var line in text.Split(new[]{'\n'},StringSplitOptions.RemoveEmptyEntries)){
        var paragraph=P(line,block.Kind=="section"?"CPSection":block.Kind=="headline"||block.Kind=="title"?"Heading2":block.Kind=="deck"?"CPDeck":"Normal");
        if(block.Kind=="headline")paragraph.Element(w+"r").AddFirst(new XElement(w+"rPr",new XElement(w+"sz",new XAttribute(w+"val",plan.Template=="digest"?30:38))));cell.Add(paragraph);
       }
      }
      if(cell.Elements().LastOrDefault()?.Name!=w+"p")cell.Add(P("","Normal"));row.Add(cell);
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
    Storage.Commit(tmp,path);
   }finally {if(File.Exists(tmp))File.Delete(tmp);}
  }
 }
}
