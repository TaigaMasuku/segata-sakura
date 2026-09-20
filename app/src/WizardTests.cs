using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
namespace SaturnApp {
internal static class WizardTests {
 static object Field(SetupWizard w,string name){return typeof(SetupWizard).GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(w);}
 static void Click(SetupWizard w,string name){((Button)Field(w,name)).PerformClick();Application.DoEvents();}
 public static void Run(string directory){
  string prior=L.Language;
  foreach(var code in L.Codes){
   var file=Path.Combine(directory,"wizard-"+code+".xml");var original=new Settings{Language="en"};
   using(var w=new SetupWizard(original,file)){
    w.Show();Application.DoEvents();
    var panel=(FlowLayoutPanel)Field(w,"body");var combo=panel.Controls.OfType<ComboBox>().Single();combo.SelectedIndex=Array.IndexOf(L.Codes,code);Application.DoEvents();
    if(L.Language!=code)throw new Exception("Live language selection failed");
    Click(w,"next");Click(w,"back");if((int)Field(w,"page")!=0)throw new Exception("Back navigation failed");
    for(int step=0;step<6;step++)Click(w,"next");
    if(w.DialogResult!=DialogResult.OK||!File.Exists(file))throw new Exception("Wizard completion failed");
   }
   var saved=Settings.Load(file);if(saved.Language!=code||!saved.SetupComplete||saved.Bios!=""||saved.Folders.Count!=0)throw new Exception("Wizard settings round trip failed");
   using(var w=new SetupWizard(saved,file)){w.Show();Application.DoEvents();Click(w,"next");var field=((FlowLayoutPanel)Field(w,"body")).Controls.OfType<FlowLayoutPanel>().SelectMany(p=>p.Controls.OfType<TextBox>()).First();field.Text="cancelled-change";Click(w,"cancel");}
   if(Settings.Load(file).BiosJapan!="")throw new Exception("Cancel wrote draft settings");
  }
  L.Language=prior;
 }
}
}
