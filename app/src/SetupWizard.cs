using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SaturnApp {
internal sealed class SetupWizard : Form {
 readonly Settings draft;
 readonly string savePath;
 readonly string previousLanguage;
 readonly string[] steps={"Language","BIOS","Game folders","Controller","Display","Ready"};
 readonly Color background=Color.FromArgb(35,37,33), gold=Color.FromArgb(204,182,119), foreground=Color.FromArgb(244,241,231);
 FlowLayoutPanel body, navigation;
 Button back,next,cancel;
 int page;
 Process native;
 public SetupWizard(Settings settings,string path){
  draft=settings.Copy();
  var bundled=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"engine","kronos.exe");if(File.Exists(bundled))draft.Engine=bundled;
  savePath=path;previousLanguage=L.Language;L.Language=L.Normalize(draft.Language);
  Size=new Size(940,660);MinimumSize=new Size(820,610);StartPosition=FormStartPosition.CenterParent;BackColor=background;ForeColor=foreground;Font=new Font("Segoe UI",10);AutoScaleMode=AutoScaleMode.Dpi;
  var icon=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"sakura-v022.ico");if(File.Exists(icon))Icon=new Icon(icon);
  var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=2,Padding=new Padding(16)};
  layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,215));layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,64));Controls.Add(layout);
  navigation=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true};layout.Controls.Add(navigation,0,0);
  body=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoScroll=true,Padding=new Padding(16,8,12,8)};layout.Controls.Add(body,1,0);
  var footer=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.RightToLeft,Padding=new Padding(4,12,4,4)};layout.Controls.Add(footer,0,1);layout.SetColumnSpan(footer,2);
  cancel=MakeButton("Cancel",()=>Close());next=MakeButton("Next",()=>Advance());back=MakeButton("Back",()=>{page--;DrawPage();});footer.Controls.Add(cancel);footer.Controls.Add(next);footer.Controls.Add(back);
  CancelButton=cancel;AcceptButton=next;
  FormClosing+=(s,e)=>{if(native!=null&&!native.HasExited){MessageBox.Show(this,L.T("Close Kronos before continuing."));e.Cancel=true;return;}if(DialogResult!=DialogResult.OK)L.Language=previousLanguage;};
  DrawPage();
 }
 Button MakeButton(string text,Action action){var b=new Button{Text=L.T(text),AutoSize=true,MinimumSize=new Size(105,38),FlatStyle=FlatStyle.Flat,BackColor=Color.FromArgb(48,50,44),ForeColor=foreground,Margin=new Padding(4)};b.FlatAppearance.BorderColor=gold;b.Click+=(s,e)=>action();return b;}
 void Clear(Control panel){foreach(Control c in panel.Controls.Cast<Control>().ToArray()){panel.Controls.Remove(c);c.Dispose();}}
 void Paragraph(string text,bool heading){body.Controls.Add(new Label{Text=L.T(text),AutoSize=true,MaximumSize=new Size(520,0),Font=new Font("Segoe UI",heading?20:10,heading?FontStyle.Bold:FontStyle.Regular),ForeColor=heading?gold:foreground,Margin=new Padding(0,0,0,20)});}
 void DrawPage(){
  SuspendLayout();Clear(body);Clear(navigation);Text="Segata Sakura — "+L.T("Setup assistant");back.Text=L.T("Back");next.Text=L.T(page==5?"Finish":"Next");cancel.Text=L.T("Cancel");back.Enabled=page>0;
  var logo=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"sakura-logo.png");if(File.Exists(logo)){var picture=new PictureBox{Size=new Size(170,115),SizeMode=PictureBoxSizeMode.Zoom,Margin=new Padding(8,8,8,24)};using(var im=Image.FromFile(logo))picture.Image=new Bitmap(im);picture.Disposed+=(s,e)=>picture.Image.Dispose();navigation.Controls.Add(picture);}
  for(int i=0;i<steps.Length;i++)navigation.Controls.Add(new Label{Text=(i+1).ToString("00")+"  "+L.T(steps[i]),AutoSize=true,MaximumSize=new Size(205,0),ForeColor=i==page?gold:foreground,Font=new Font("Segoe UI",10,i==page?FontStyle.Bold:FontStyle.Regular),Margin=new Padding(8,10,0,10)});
  Paragraph(page==0?"Welcome to Segata Sakura":page==5?"Your library is ready":steps[page],true);
  if(page==0){
   Paragraph("Choose your application language. You can reopen this assistant from Settings.",false);
   var languages=new ComboBox{BackColor=Color.FromArgb(48,50,44),ForeColor=foreground,DropDownStyle=ComboBoxStyle.DropDownList,Width=440,Margin=new Padding(0,0,0,24)};LanguageFlags.Attach(languages);languages.Items.AddRange(L.Names);languages.SelectedIndex=Array.IndexOf(L.Codes,L.Normalize(draft.Language));languages.SelectedIndexChanged+=(s,e)=>{draft.Language=L.Codes[languages.SelectedIndex];L.Language=draft.Language;BeginInvoke(new Action(DrawPage));};body.Controls.Add(languages);
   Paragraph("Kronos is included. Games and BIOS files are not included.",false);
  }else if(page==1){
   Paragraph("Select your Saturn BIOS file, or configure it later.",false);
   var fields=new[]{draft.BiosJapan,draft.BiosUsa,draft.BiosEurope};var labels=new[]{"Japan (NTSC-J)","USA (NTSC-U)","Europe (PAL)"};
   for(int i=0;i<3;i++){int region=i;Paragraph(labels[i],false);var row=new FlowLayoutPanel{AutoSize=true,Width=515,Margin=new Padding(0,0,0,8)};var field=new TextBox{Text=fields[i],Width=350,BackColor=Color.FromArgb(48,50,44),ForeColor=foreground,Margin=new Padding(0,8,4,0)};field.TextChanged+=(a,b)=>{if(region==0)draft.BiosJapan=field.Text.Trim();else if(region==1)draft.BiosUsa=field.Text.Trim();else draft.BiosEurope=field.Text.Trim();};row.Controls.Add(field);row.Controls.Add(MakeButton("Browse…",()=>{using(var f=new OpenFileDialog{Filter=L.T("BIOS files|*.bin;*.rom|All files|*.*")})if(f.ShowDialog(this)==DialogResult.OK)field.Text=f.FileName;}));body.Controls.Add(row);}
   var hle=new CheckBox{Text=L.T("Use emulated BIOS (experimental compatibility)"),Checked=draft.Hle,AutoSize=true,MaximumSize=new Size(500,0),Margin=new Padding(0,12,0,12)};hle.CheckedChanged+=(a,b)=>draft.Hle=hle.Checked;body.Controls.Add(hle);Paragraph("You can leave this empty. A BIOS will be requested before playing.",false);
  }else if(page==2){
   Paragraph("Add folders containing CUE and its tracks, ISO or CHD files. Subfolders are scanned too.",false);
   var list=new ListBox{Width=490,Height=150,HorizontalScrollbar=true,BackColor=background,ForeColor=foreground};list.Items.AddRange(draft.Folders.ToArray());body.Controls.Add(list);
   body.Controls.Add(MakeButton("+ Folder",()=>{try{var folder=FolderPicker.Choose(this);if(folder!=null&&!draft.Folders.Contains(folder,StringComparer.OrdinalIgnoreCase)){draft.Folders.Add(folder);list.Items.Add(folder);}}catch(Exception e){MessageBox.Show(this,e.Message);}}));
   body.Controls.Add(MakeButton("Remove",()=>{if(list.SelectedIndex>=0){draft.Folders.RemoveAt(list.SelectedIndex);list.Items.RemoveAt(list.SelectedIndex);}}));Paragraph("You can add games later from the library.",false);
  }else if(page==3){
   Paragraph("After this assistant, open Settings > Controller to assign keys, mouse buttons or USB controllers directly in Segata Sakura.",false);
  }else if(page==4){
   var full=new CheckBox{Text=L.T("Start games in fullscreen"),AutoSize=true,MaximumSize=new Size(500,0),Checked=draft.Fullscreen,Margin=new Padding(0,0,0,24)};full.CheckedChanged+=(s,e)=>draft.Fullscreen=full.Checked;body.Controls.Add(full);
   Paragraph("The library uses cover cards. Resolution, rendering and audio are configured in Kronos.",false);body.Controls.Add(MakeButton("Open Kronos settings",OpenNative));
  }else{
   Paragraph(L.T("Language")+": "+L.Names[Array.IndexOf(L.Codes,L.Normalize(draft.Language))],false);
   Paragraph(L.T("BIOS")+": "+(draft.Hle?L.T("Emulated BIOS"):RegionalBios.HasBios(draft)?L.T("Automatic"):L.T("Not configured yet")),false);
   Paragraph(L.T("Game folders")+": "+draft.Folders.Count,false);Paragraph("Finish saves these settings. You can change them at any time.",false);
  }
  ResumeLayout(true);
 }
 void OpenNative(){try{if(native!=null&&!native.HasExited){MessageBox.Show(this,L.T("Close Kronos before continuing."));return;}if(!File.Exists(draft.Engine))throw new IOException(L.T("Kronos executable was not found. Reinstall the application."));native=Process.Start(new ProcessStartInfo(draft.Engine){WorkingDirectory=Path.GetDirectoryName(draft.Engine),UseShellExecute=false});}catch(Exception e){MessageBox.Show(this,e.Message,"Segata Sakura");}}
 void Advance(){try{
  if(native!=null&&!native.HasExited)throw new IOException(L.T("Close Kronos before continuing."));
  if(page==1&&!draft.Hle&&new[]{draft.BiosJapan,draft.BiosUsa,draft.BiosEurope}.Any(b=>!string.IsNullOrWhiteSpace(b)&&!File.Exists(b)))throw new IOException(L.T("The selected BIOS file does not exist."));
  if(page<5){page++;DrawPage();return;}
  draft.SetupComplete=true;draft.Save(savePath);DialogResult=DialogResult.OK;Close();
 }catch(Exception e){MessageBox.Show(this,e.Message,"Segata Sakura",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
 // Deterministic layout capture used by local UI checks; no settings are written.
 internal void CapturePages(string directory){Show();for(int language=0;language<L.Codes.Length;language++){draft.Language=L.Codes[language];L.Language=draft.Language;for(page=0;page<steps.Length;page++){DrawPage();Application.DoEvents();using(var bmp=new Bitmap(Width,Height)){DrawToBitmap(bmp,new Rectangle(0,0,Width,Height));bmp.Save(Path.Combine(directory,"wizard-"+draft.Language+"-"+page+".png"));}}}Close();}
}
}
