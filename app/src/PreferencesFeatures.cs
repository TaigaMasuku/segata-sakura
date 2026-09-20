using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
namespace SaturnApp {
internal sealed partial class PreferencesForm {
 UsbInput usb;string usbError="";bool usbTried;int presetIndex,deviceIndex;int lastVolume=100;
 void DrawController(){
  if(!usbTried){usbTried=true;try{usb=new UsbInput();}catch(Exception e){usbError=e.Message;}}
  TextLine("Player 1 — integrated mapping");
  Combo("Preset",ControllerInput.Presets.Select(L.T).ToArray(),presetIndex,i=>presetIndex=i);
  string[] devices=usb!=null&&usb.Names.Count>0?usb.Names.ToArray():new[]{L.T("No USB controller detected")};deviceIndex=Math.Min(deviceIndex,devices.Length-1);
  Combo("USB device",devices,deviceIndex,i=>deviceIndex=i);
  var actions=new FlowLayoutPanel{AutoSize=true,Width=570};actions.Controls.Add(Button("Apply preset",()=>{try{
   int type=2;uint[] codes;if(presetIndex<3)codes=ControllerInput.Keyboard(presetIndex==1,presetIndex==2);else if(presetIndex==3){type=227;codes=new[]{ControllerInput.Mouse(MouseButtons.Left),ControllerInput.Mouse(MouseButtons.Middle),ControllerInput.Mouse(MouseButtons.Right),ControllerInput.Key(Keys.Enter),0x40000000u};}else{if(usb==null||usb.Names.Count==0)throw new IOException(L.T("No USB controller detected"));codes=usb.Preset(deviceIndex);if(codes.Any(c=>c==0))throw new IOException(L.T("This adapter has no automatic mapping. Assign its buttons manually."));}
   ControllerInput.Apply(engine,type,codes);Draw();
  }catch(Exception e){MessageBox.Show(this,e.Message);}}));
  actions.Controls.Add(Button("Refresh devices",()=>{if(usb!=null)usb.Dispose();usb=null;usbTried=false;usbError="";Draw();}));body.Controls.Add(actions);
  string raw;int typeId=engine.Values.TryGetValue("Input\\Port\\1\\Id\\1\\Type",out raw)&&raw=="227"?227:2;
  var names=typeId==227?new[]{L.T("Left button"),L.T("Middle button"),L.T("Right button"),"Start",L.T("Mouse movement")}:ControllerInput.Buttons;
  var grid=new TableLayoutPanel{AutoSize=true,ColumnCount=4,RowCount=(names.Length+1)/2,Margin=new Padding(0,8,0,12)};grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,95));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,175));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,95));grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,175));
  for(int i=0;i<names.Length;i++){int keyId=i+(typeId==227?13:0);string value;uint code=engine.Values.TryGetValue(ControllerInput.Prefix(typeId)+keyId,out value)&&uint.TryParse(value,out code)?code:0;
   grid.Controls.Add(new Label{Text=names[i],AutoSize=true,Margin=new Padding(0,12,4,0)},(i%2)*2,i/2);
   var assign=Button(ControllerInput.Describe(code),()=>{using(var capture=new InputCapture(usb,usb!=null&&usb.Names.Count>0?deviceIndex:-1,keyId==17))if(capture.ShowDialog(this)==DialogResult.OK){engine.Values["Input\\PerCore"]="3";engine.Values["Input\\Port\\1\\Id\\1\\Type"]=typeId.ToString();engine.Values[ControllerInput.Prefix(typeId)+keyId]=capture.Code.ToString();Draw();}});assign.AutoSize=false;assign.Width=165;grid.Controls.Add(assign,(i%2)*2+1,i/2);
  }body.Controls.Add(grid);
  TextLine("Click a binding to change it. Presets are editable; USB adapters may need manual mapping.");TextLine("Mouse movement requires a mouse-compatible Saturn game. USB device order must remain unchanged.");if(usbError!="")TextLine(usbError);
 }
 void BiosField(string title,string value,Action<string> set){TextLine(title);var row=new FlowLayoutPanel{AutoSize=true,Width=585};var box=new TextBox{Text=value,Width=400,BackColor=panel,ForeColor=ink,Margin=new Padding(0,9,8,0)};box.TextChanged+=(s,e)=>set(box.Text.Trim());row.Controls.Add(box);row.Controls.Add(Button("Browse…",()=>{using(var d=new OpenFileDialog{Filter=L.T("BIOS files|*.bin;*.rom|All files|*.*")})if(d.ShowDialog(this)==DialogResult.OK)box.Text=d.FileName;}));body.Controls.Add(row);}
 void DrawBios(){
  BiosField("Japan (NTSC-J)",draft.BiosJapan,s=>draft.BiosJapan=s);BiosField("USA (NTSC-U)",draft.BiosUsa,s=>draft.BiosUsa=s);BiosField("Europe (PAL)",draft.BiosEurope,s=>draft.BiosEurope=s);
  var codes=new[]{"auto","JP","US","EU"};Combo("BIOS selection",new[]{L.T("Automatic"),L.T("Japan (NTSC-J)"),L.T("USA (NTSC-U)"),L.T("Europe (PAL)")},Array.IndexOf(codes,draft.BiosRegion),i=>draft.BiosRegion=codes[i]);
  var regions=new[]{"JP","US","EU"};Combo("Fallback region (CHD / unknown / multi-region)",new[]{L.T("Japan (NTSC-J)"),L.T("USA (NTSC-U)"),L.T("Europe (PAL)")},Array.IndexOf(regions,draft.DefaultRegion),i=>draft.DefaultRegion=regions[i]);
  Check("Use emulated BIOS (experimental compatibility)",draft.Hle,v=>draft.Hle=v);if(!string.IsNullOrEmpty(draft.Bios))BiosField("Legacy BIOS (used until regional files are configured)",draft.Bios,s=>draft.Bios=s);
  TextLine("Automatic selection reads CUE/ISO headers. Supply your own BIOS files; filenames alone do not prove their region.");
 }
 void DrawAudio(){
  TextLine("Output volume");int volume;if(!int.TryParse(engine.Values["Sound\\Volume"],out volume))volume=100;
  var row=new FlowLayoutPanel{AutoSize=true,Width=580};var slider=new TrackBar{Minimum=0,Maximum=100,Value=Math.Max(0,Math.Min(100,volume)),TickStyle=TickStyle.None,Width=470,SmallChange=1,LargeChange=10};var percent=new Label{Text=slider.Value+"%",AutoSize=true,Margin=new Padding(8,10,0,0)};row.Controls.Add(slider);row.Controls.Add(percent);body.Controls.Add(row);
  slider.ValueChanged+=(s,e)=>{engine.Values["Sound\\Volume"]=slider.Value.ToString();percent.Text=slider.Value+"%";};
  body.Controls.Add(Button("Mute / restore",()=>{if(slider.Value>0){lastVolume=slider.Value;slider.Value=0;}else slider.Value=lastVolume;}));body.Controls.Add(Button("Reset volume",()=>slider.Value=100));TextLine("Changes apply to the next game launch.");
 }
 void DrawEmulation(){
  Check("Fast-forward at launch (unlimited)",engine.Values["General\\EnableVSync"]=="false",v=>engine.Values["General\\EnableVSync"]=v?"false":"true");
  TextLine("Kronos uses its VSync/frame limiter for fast-forward. F4 toggles it during play with the default shortcuts. Speed depends on your computer; no fixed multiplier is promised.");
  TextLine("Disabling fast-forward restores VSync. This is the same setting as the VSync option in Graphics.");
 }
}
}
