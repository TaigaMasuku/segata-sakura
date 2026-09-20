using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
namespace SaturnApp {
internal static class NewFeaturesTests {
 internal static void Run(string dir){
  UsbInput.TestVirtual();var jp=Path.Combine(dir,"jp.bin");var us=Path.Combine(dir,"us.bin");var eu=Path.Combine(dir,"eu.bin");foreach(var file in new[]{jp,us,eu})File.WriteAllText(file,"bios fixture");var s=new Settings{BiosJapan=jp,BiosUsa=us,BiosEurope=eu,Engine=Path.Combine(dir,"kronos.exe")};
  var iso=Path.Combine(dir,"region.iso");var bytes=new byte[2048];Encoding.ASCII.GetBytes("SEGA SEGASATURN").CopyTo(bytes,0);Encoding.ASCII.GetBytes("U               ").CopyTo(bytes,64);File.WriteAllBytes(iso,bytes);if(RegionalBios.Select(s,iso)!=us)throw new Exception("USA BIOS selection");
  var cue=Path.Combine(dir,"region.cue");File.WriteAllText(cue,"FILE \"region.iso\" BINARY\n TRACK 01 MODE1/2048\n INDEX 01 00:00:00");if(RegionalBios.Select(s,cue)!=us)throw new Exception("CUE BIOS selection");
  bytes[64]=(byte)'E';File.WriteAllBytes(iso,bytes);if(RegionalBios.Select(s,iso)!=eu)throw new Exception("PAL BIOS selection");
  s.BiosRegion="JP";if(RegionalBios.Select(s,iso)!=jp)throw new Exception("Manual BIOS selection");s.BiosRegion="auto";s.DefaultRegion="EU";if(RegionalBios.Select(s,"unknown.chd")!=eu)throw new Exception("CHD region fallback");
  s.BiosEurope="";bool missing=false;try{RegionalBios.Select(s,iso);}catch(IOException){missing=true;}if(!missing)throw new Exception("Missing BIOS was hidden");s.BiosEurope=eu;
  var xml=Path.Combine(dir,"regional.xml");s.Save(xml);var loaded=Settings.Load(xml);if(loaded.BiosJapan!=jp||loaded.BiosUsa!=us||loaded.BiosEurope!=eu||loaded.DefaultRegion!="EU")throw new Exception("Regional persistence");
  var keyboard=ControllerInput.Keyboard(false,false);if(keyboard.Length!=13||keyboard[0]!=87||keyboard[6]!=0x1000004||keyboard[7]!=49)throw new Exception("Keyboard preset");if(ControllerInput.Keyboard(true,false)[3]!=81||ControllerInput.Keyboard(false,true)[7]!=0x80000001)throw new Exception("AZERTY/mouse preset");
  if(UsbInput.Encode(1,new UsbInput.Binding{Type=1,Index=0})!=0x40001||UsbInput.Encode(0,new UsbInput.Binding{Type=3,Index=0,HatMask=1})!=0x200010||UsbInput.Encode(0,new UsbInput.Binding{Type=2,Index=4})!=0x110004)throw new Exception("SDL binding encoding");
  var pref=new EnginePreferences(s.Engine);ControllerInput.Apply(pref,2,keyboard);pref.Save(s,xml);var ini=EnginePreferences.Read(Path.Combine(dir,"kronos.ini"));if(ini["Input\\Port\\1\\Id\\1\\Type"]!="2"||ini[ControllerInput.Prefix(2)+"7"]!="49")throw new Exception("Controller persistence");
  ControllerInput.Apply(pref,227,new uint[]{0x80000001,0x80000004,0x80000002,0x1000004,0x40000000});pref=new EnginePreferences(s.Engine);ControllerInput.Apply(pref,227,new uint[]{0x80000001,0x80000004,0x80000002,0x1000004,0x40000000});pref.Save(s,xml);if(EnginePreferences.Read(Path.Combine(dir,"kronos.ini"))[ControllerInput.Prefix(227)+"17"]!="1073741824")throw new Exception("Mouse movement binding");
  using(var ui=new PreferencesForm(s,xml)){ui.Show();Application.DoEvents();var flags=BindingFlags.NonPublic|BindingFlags.Instance;var side=(FlowLayoutPanel)typeof(PreferencesForm).GetField("side",flags).GetValue(ui);((Button)side.Controls[4]).PerformClick();Application.DoEvents();var body=(FlowLayoutPanel)typeof(PreferencesForm).GetField("body",flags).GetValue(ui);var slider=body.Controls.OfType<FlowLayoutPanel>().SelectMany(x=>x.Controls.OfType<TrackBar>()).Single();slider.Value=37;((Button)typeof(PreferencesForm).GetField("save",flags).GetValue(ui)).PerformClick();Application.DoEvents();if(ui.DialogResult!=DialogResult.OK)throw new Exception("Audio slider save");}
  if(EnginePreferences.Read(Path.Combine(dir,"kronos.ini"))["Sound\\Volume"]!="37")throw new Exception("Audio slider did not persist");
 }
}
}
