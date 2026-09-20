using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
namespace SaturnApp {
internal sealed partial class PreferencesForm:Form {
 readonly Settings draft;readonly EnginePreferences engine;readonly string path,oldLanguage;
 readonly string[] pages={"Interface","Game folders","BIOS","Graphics and filters","Audio","Controller","Emulation"};
 readonly Color bg=Color.FromArgb(35,37,33),panel=Color.FromArgb(48,50,44),gold=Color.FromArgb(204,182,119),ink=Color.FromArgb(244,241,231);
 FlowLayoutPanel body,side;Button save,cancel;int page;
 internal PreferencesForm(Settings current,string settingsPath){
  draft=current.Copy();path=settingsPath;oldLanguage=L.Language;engine=new EnginePreferences(draft.Engine);engine.Values["Video\\Fullscreen"]=draft.Fullscreen?"true":"false";
  Text="Segata Sakura — "+L.T("Settings");Size=new Size(1000,730);MinimumSize=new Size(870,650);BackColor=bg;ForeColor=ink;Font=new Font("Segoe UI",10);StartPosition=FormStartPosition.CenterParent;AutoScaleMode=AutoScaleMode.Dpi;
  var icon=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"sakura-v022.ico");if(File.Exists(icon))Icon=new Icon(icon);
  var layout=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(16),ColumnCount=2,RowCount=2};layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,230));layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,60));Controls.Add(layout);
  side=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true};body=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(14)};layout.Controls.Add(side,0,0);layout.Controls.Add(body,1,0);
  var bottom=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.RightToLeft,Padding=new Padding(4)};layout.Controls.Add(bottom,0,1);layout.SetColumnSpan(bottom,2);cancel=Button("Cancel",()=>Close());save=Button("Save",Commit);bottom.Controls.Add(cancel);bottom.Controls.Add(save);CancelButton=cancel;
  FormClosed+=(s,e)=>{if(usb!=null)usb.Dispose();};
  FormClosing+=(s,e)=>{if(DialogResult!=DialogResult.OK)L.Language=oldLanguage;};Draw();
 }
 Button Button(string key,Action click){var b=new Button{Text=L.T(key),AutoSize=true,MinimumSize=new Size(115,38),FlatStyle=FlatStyle.Flat,BackColor=panel,ForeColor=ink,Margin=new Padding(3,5,3,8),Padding=new Padding(8,2,8,2)};b.FlatAppearance.BorderColor=gold;b.Click+=(s,e)=>click();return b;}
 static void Clear(Control p){foreach(var c in p.Controls.Cast<Control>().ToArray()){p.Controls.Remove(c);c.Dispose();}}
 void TextLine(string key,bool title=false){body.Controls.Add(new Label{Text=L.T(key),AutoSize=true,MaximumSize=new Size(600,0),ForeColor=title?gold:ink,Font=new Font("Segoe UI",title?20:10,title?FontStyle.Bold:FontStyle.Regular),Margin=new Padding(0,0,0,title?24:12)});}
 CheckBox Check(string key,bool value,Action<bool> set){var b=new CheckBox{Text=L.T(key),Checked=value,AutoSize=true,MaximumSize=new Size(590,0),Margin=new Padding(0,6,0,14)};b.CheckedChanged+=(s,e)=>set(b.Checked);body.Controls.Add(b);return b;}
 ComboBox Combo(string label,string[] names,int selected,Action<int> set){TextLine(label);var c=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,DrawMode=DrawMode.OwnerDrawFixed,BackColor=panel,ForeColor=ink,Width=500,ItemHeight=25,Margin=new Padding(0,0,0,16)};c.Items.AddRange(names);c.SelectedIndex=selected;
  c.DrawItem+=(s,e)=>LanguageFlags.PaintItem(c,e,label=="Language");c.SelectedIndexChanged+=(s,e)=>set(c.SelectedIndex);body.Controls.Add(c);return c;}
 void Option(string label,string key,int[] ids,string[] names){int value;int.TryParse(engine.Values[key],out value);var keys=ids.ToList();var labels=names.Select(L.T).ToList();if(!keys.Contains(value)){keys.Add(value);labels.Add(L.T("Custom")+" ("+value+")");}Combo(label,labels.ToArray(),keys.IndexOf(value),i=>engine.Values[key]=keys[i].ToString(System.Globalization.CultureInfo.InvariantCulture));}
 void Draw(){Clear(side);Clear(body);save.Text=L.T("Save");cancel.Text=L.T("Cancel");Text="Segata Sakura — "+L.T("Settings");
  for(int i=0;i<pages.Length;i++){int n=i;var b=Button(pages[i],()=>{page=n;Draw();});b.Width=210;b.AutoSize=false;if(i==page){b.BackColor=gold;b.ForeColor=bg;}side.Controls.Add(b);}TextLine(pages[page],true);
  if(page==0){Combo("Language",L.Names,Array.IndexOf(L.Codes,L.Normalize(draft.Language)),i=>{draft.Language=L.Codes[i];L.Language=draft.Language;BeginInvoke(new Action(Draw));});Check("Start games in fullscreen",draft.Fullscreen,v=>{draft.Fullscreen=v;engine.Values["Video\\Fullscreen"]=v?"true":"false";});Check("Minimize library when a game starts",draft.MinimizeOnPlay,v=>draft.MinimizeOnPlay=v);TextLine("Changes apply to the next game launch.");}
  if(page==1){Check("Scan subfolders",draft.Recursive,v=>draft.Recursive=v);var list=new ListBox{Width=560,Height=200,HorizontalScrollbar=true,BackColor=panel,ForeColor=ink};list.Items.AddRange(draft.Folders.ToArray());body.Controls.Add(list);body.Controls.Add(Button("+ Folder",()=>{try{var f=FolderPicker.Choose(this);if(f!=null&&!draft.Folders.Contains(f,StringComparer.OrdinalIgnoreCase)){draft.Folders.Add(f);list.Items.Add(f);}}catch(Exception e){MessageBox.Show(this,e.Message);}}));body.Controls.Add(Button("Remove",()=>{if(list.SelectedIndex>=0){draft.Folders.RemoveAt(list.SelectedIndex);list.Items.RemoveAt(list.SelectedIndex);}}));TextLine("Removing a folder does not delete your games.");}
  if(page==2)DrawBios();
  if(page==3){
   Option("Image filter","Video\\filter_type",new[]{0,1,2,3,5,6},new[]{"None","Bilinear","Bicubic","Adaptive deinterlacing","Bob deinterlacing","Scanlines"});
   Option("Upscaling filter","Video\\upscale_type",new[]{0,1,2,3},new[]{"None","HQ4x","4xBRZ","6xBRZ"});
   Option("Internal resolution","Video\\resolution_mode",new[]{1,8,32,64},new[]{"1x — Saturn","2x","4x","Window resolution"});
   Option("Aspect ratio","Video\\AspectRatio",new[]{0,1,2,3},new[]{"Original","Stretch","Pixel perfect — fullscreen","Pixel perfect"});
   Check("Vertical sync (VSync)",engine.Values["General\\EnableVSync"]=="true",v=>engine.Values["General\\EnableVSync"]=v?"true":"false");Check("Show FPS",engine.Values["General\\ShowFPS"]=="true",v=>engine.Values["General\\ShowFPS"]=v?"true":"false");TextLine("Higher resolutions and upscaling filters require more GPU power. Effects depend on the Kronos renderer.");
  }
  if(page==4)DrawAudio();
  if(page==5)DrawController();
  if(page==6)DrawEmulation();
 }
 void Commit(){try{if(!draft.Hle&&new[]{draft.Bios,draft.BiosJapan,draft.BiosUsa,draft.BiosEurope}.Any(b=>!string.IsNullOrWhiteSpace(b)&&!File.Exists(b)))throw new IOException(L.T("The selected BIOS file does not exist."));engine.Save(draft,path);DialogResult=DialogResult.OK;Close();}catch(Exception e){MessageBox.Show(this,e.Message,"Segata Sakura",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
 internal void CapturePages(string directory){Show();for(page=0;page<pages.Length;page++){Draw();Application.DoEvents();using(var b=new Bitmap(Width,Height)){DrawToBitmap(b,new Rectangle(0,0,Width,Height));b.Save(Path.Combine(directory,"settings-"+page+".png"));}}Close();}
}
}
