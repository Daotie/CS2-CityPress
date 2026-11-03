using System;
using System.Collections.Generic;
using System.IO;
using Colossal;
using Game.SceneFlow;
using Newtonsoft.Json;

namespace CityPress.Systems {
 internal sealed class CityPressLocalization : IDictionarySource {
  readonly Dictionary<string,string> entries;
  CityPressLocalization(Dictionary<string,string> entries){this.entries=entries;}
  public IEnumerable<KeyValuePair<string,string>> ReadEntries(IList<IDictionaryEntryError> errors,Dictionary<string,int> indexCounts)=>entries;
  public void Unload(){}
  internal static void Register(){
   foreach(var locale in new[]{"en-US","zh-HANS"}){
    using(var stream=typeof(CityPressLocalization).Assembly.GetManifestResourceStream("CityPress.lang."+locale+".json")){
     if(stream==null)throw new InvalidOperationException("Missing bundled locale: "+locale);
     using(var reader=new StreamReader(stream)){
      var entries=JsonConvert.DeserializeObject<Dictionary<string,string>>(reader.ReadToEnd());
      GameManager.instance.localizationManager.AddSource(locale,new CityPressLocalization(entries));
     }
    }
   }
  }
 }
}
