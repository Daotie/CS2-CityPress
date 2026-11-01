using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CityPress.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

static class LocalizationTests {
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);Console.WriteLine("PASS "+message);}
 static Dictionary<string,string> Load(string root,string locale)=>JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText(Path.Combine(root,"lang",locale+".json"),System.Text.Encoding.UTF8));
 public static void Run(string root) {
  var en=Load(root,"en-US");var zh=Load(root,"zh-HANS");
  Check(en.Count>100&&en.Keys.OrderBy(key=>key,StringComparer.Ordinal).SequenceEqual(zh.Keys.OrderBy(key=>key,StringComparer.Ordinal)),"中英文语言文件键一致且包含完整词条");
  Check(en.Keys.All(key=>key.StartsWith("CityPress.",StringComparison.Ordinal))&&en.Values.All(value=>!string.IsNullOrWhiteSpace(value)),"语言文件只包含 CityPress 命名空间且无空值");

  var uiSource=File.ReadAllText(Path.Combine(root,"UI","src","index.tsx"));
  Check(!Regex.IsMatch(uiSource,"[\\u4e00-\\u9fff]"),"游戏内界面不再硬编码中文");
  var uiKeys=Regex.Matches(uiSource,@"\bt\('([A-Za-z0-9.]+)'").SelectMany(match=>match.Groups.Cast<Group>().Skip(1).Select(group=>"CityPress.UI."+group.Value))
   .Concat(Regex.Matches(uiSource,@"\btr\('(CityPress\.[A-Za-z0-9.]+)'").SelectMany(match=>match.Groups.Cast<Group>().Skip(1).Select(group=>group.Value)))
   .Distinct().OrderBy(key=>key,StringComparer.Ordinal).ToList();
  var missingUi=uiKeys.Where(key=>!en.ContainsKey(key)).ToList();
  Check(missingUi.Count==0,"界面引用的词条全部存在"+(missingUi.Count==0?"":":"+string.Join(",",missingUi)));

  var i18nSource=File.ReadAllText(Path.Combine(root,"UI","src","i18n.ts"));
  var themeKeys=Regex.Matches(i18nSource,@"'Theme\.([A-Za-z]+)'").SelectMany(match=>match.Groups.Cast<Group>().Skip(1).Select(group=>"CityPress.UI.Theme."+group.Value)).Distinct();
  Check(themeKeys.All(key=>en.ContainsKey(key)),"界面主题标签词条存在");

  var codeKeys=new List<string>();
  foreach(var file in Directory.GetFiles(Path.Combine(root,"Core"),"*.cs").Concat(Directory.GetFiles(Path.Combine(root,"Systems"),"*.cs")))codeKeys.AddRange(Regex.Matches(File.ReadAllText(file),@"(CityPress\.(?:Status|Error)\.[A-Za-z0-9]+)").SelectMany(match=>match.Groups.Cast<Group>().Skip(1).Select(group=>group.Value)));
  foreach(var key in codeKeys.Distinct())Check(en.ContainsKey(key)&&zh.ContainsKey(key),"后端状态/错误词条存在："+key);
  Check(codeKeys.Distinct().Count()>=30,"后端状态与错误使用稳定词条而不是硬编码文案");

  Check(I18n.MostlyChinese("汐澜市常住人口突破四十万")&&!I18n.MostlyChinese("The city population passed 400,000"),"中英文内容语言检测");
  var failure=new LocalizedException("CityPress.Error.ApiHttp","API 请求失败（HTTP {0}）",500);
  Check(failure.Key=="CityPress.Error.ApiHttp"&&failure.Message.Contains("500"),"本地化异常保留词条并格式化回退文案");

  var english=new Newspaper{Name="Metro Daily",Day="2027-01-12",Headline="Transit plan approved by the city",Deck="The council backed a new line and service plan.",Articles=new List<Article>{
   new Article{Section="City",Genre="消息",Title="New line approved",Body="The city approved a new transit line and a service plan for the coming year."}
  }};
  var output=Path.Combine(root,"artifacts","sample","英文版式校样.docx");DocxExporter.Save(output,english);
  using(var zip=ZipFile.OpenRead(output))using(var stream=zip.GetEntry("word/document.xml").Open()){
   var doc=XDocument.Load(stream);XNamespace w="http://schemas.openxmlformats.org/wordprocessingml/2006/main";
   var text=string.Concat(doc.Descendants(w+"t").Select(node=>node.Value));
   Check(text.Contains("CITY NEWS")&&text.Contains("CITY CHRONICLE")&&text.Contains("DAILY")&&text.Contains("January 12, 2027"),"英文报纸的 Word 版签和日期已本地化");
   Check(!text.Contains("城市新闻")&&!text.Contains("城市纪事")&&!text.Contains("电子刊"),"英文报纸不再混入中文版签");
  }
 }
}
