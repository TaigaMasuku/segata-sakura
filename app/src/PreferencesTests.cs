using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
namespace SaturnApp {
internal static class PreferencesTests {
 internal static void Run(string dir){
  var engine=Path.Combine(dir,"kronos.exe");var ini=Path.Combine(dir,"kronos.ini");var settingsFile=Path.Combine(dir,"preferences.xml");
  var initial="; preserved comment\r\n[1.0]\r\nInput\\Mapping=custom\r\nVideo\\filter_type=0\r\nSound\\Volume=72\r\n[Other]\r\nName=été\r\n";File.WriteAllText(ini,initial);
  var p=new EnginePreferences(engine);p.Values["Video\\filter_type"]="6";p.Values["Video\\upscale_type"]="2";p.Values["Video\\resolution_mode"]="8";p.Values["Video\\AspectRatio"]="3";p.Values["General\\EnableVSync"]="false";p.Values["Sound\\Volume"]="0";
  var s=new Settings{Engine=engine,Recursive=false,MinimizeOnPlay=true};p.Save(s,settingsFile);var result=File.ReadAllText(ini);var values=EnginePreferences.Read(ini);
  if(values["Video\\filter_type"]!="6"||values["Video\\resolution_mode"]!="8"||values["Video\\upscale_type"]!="2"||values["Sound\\Volume"]!="0"||values["General\\EnableVSync"]!="false")throw new Exception("Engine settings not applied");
  if(!result.Contains("Input\\Mapping=custom")||!result.Contains("Name=été")||!result.Contains("; preserved comment"))throw new Exception("Unrelated INI content lost");
  if(!File.Exists(ini+".segata.bak"))throw new Exception("INI backup missing");var reloaded=Settings.Load(settingsFile);if(reloaded.Recursive||!reloaded.MinimizeOnPlay)throw new Exception("New settings persistence failed");
  var stable=File.ReadAllBytes(ini);new EnginePreferences(engine).Save(s,settingsFile);if(!stable.SequenceEqual(File.ReadAllBytes(ini)))throw new Exception("Unchanged INI rewritten");
  p=new EnginePreferences(engine);p.Values["Sound\\Volume"]="50";bool rollback=false;try{p.Save(s,Path.Combine(dir,"missing-parent","settings.xml"));}catch(IOException){rollback=true;}if(!rollback||!stable.SequenceEqual(File.ReadAllBytes(ini)))throw new Exception("Failed save did not roll back INI");
  p=new EnginePreferences(engine);p.Values["Sound\\Volume"]="30";File.WriteAllText(ini,File.ReadAllText(ini).Replace("Sound\\Volume=0","Sound\\Volume=20"));bool conflict=false;try{p.Save(s,settingsFile);}catch(IOException){conflict=true;}if(!conflict||EnginePreferences.Read(ini)["Sound\\Volume"]!="20")throw new Exception("Concurrent edit was overwritten");
  var nested=Path.Combine(dir,"nested");Directory.CreateDirectory(nested);File.WriteAllText(Path.Combine(nested,"nested.iso"),"fixture");var errors=new System.Collections.Generic.List<string>();if(Library.Scan(new[]{dir},errors,false).Any(g=>g.Path.EndsWith("nested.iso"))||!Library.Scan(new[]{dir},errors,true).Any(g=>g.Path.EndsWith("nested.iso")))throw new Exception("Recursive setting ignored");
  var old=L.Language;foreach(var lang in L.Codes){L.Language=lang;var uiSettings=new Settings{Engine=engine,Language=lang};using(var ui=new PreferencesForm(uiSettings,settingsFile)){
   ui.Show();Application.DoEvents();var side=(FlowLayoutPanel)typeof(PreferencesForm).GetField("side",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(ui);for(int i=0;i<6;i++){((Button)side.Controls[i]).PerformClick();Application.DoEvents();}
   ((Button)side.Controls[3]).PerformClick();Application.DoEvents();var body=(FlowLayoutPanel)typeof(PreferencesForm).GetField("body",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(ui);var combos=body.Controls.OfType<ComboBox>().ToArray();if(combos.Length!=4||combos.Any(c=>c.SelectedIndex<0||string.IsNullOrEmpty(c.Text)))throw new Exception("Empty graphics selection");combos[0].SelectedIndex=5;combos[1].SelectedIndex=1;combos[2].SelectedIndex=2;
   ((Button)typeof(PreferencesForm).GetField("save",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(ui)).PerformClick();Application.DoEvents();if(ui.DialogResult!=DialogResult.OK)throw new Exception("Settings dialog save failed");
   var applied=EnginePreferences.Read(ini);if(applied["Video\\filter_type"]!="6"||applied["Video\\upscale_type"]!="1"||applied["Video\\resolution_mode"]!="32")throw new Exception("UI filter IDs incorrect");
  }}L.Language=old;
 }
}
}
