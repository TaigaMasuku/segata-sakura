using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace SaturnApp {
internal static class ControllerInput {
 internal static readonly string[] Buttons={"↑","→","↓","←","R","L","Start","A","B","C","X","Y","Z"};
 internal static readonly string[] Presets={"Keyboard — Mednafen","Keyboard — AZERTY","Keyboard + mouse buttons","Saturn mouse","PlayStation 1 (USB adapter)","PlayStation 2 (USB adapter)","PlayStation 3","PlayStation 4","PlayStation 5","Xbox"};
 internal static string Prefix(int type){return "Input\\Port\\1\\Id\\1\\Controller\\"+type+"\\Key\\";}
 internal static uint Key(Keys k){if(k>=Keys.A&&k<=Keys.Z||k>=Keys.D0&&k<=Keys.D9)return (uint)k;if(k>=Keys.NumPad0&&k<=Keys.NumPad9)return (uint)(48+k-Keys.NumPad0);switch(k){case Keys.Up:return 0x1000013;case Keys.Right:return 0x1000014;case Keys.Down:return 0x1000015;case Keys.Left:return 0x1000012;case Keys.Enter:return 0x1000004;case Keys.Space:return 32;case Keys.Tab:return 0x1000001;case Keys.Back:return 0x1000003;case Keys.ShiftKey:return 0x1000020;case Keys.ControlKey:return 0x1000021;case Keys.Menu:return 0x1000023;}if(k>=Keys.F1&&k<=Keys.F12)return (uint)(0x1000030+k-Keys.F1);return 0;}
 internal static uint Mouse(MouseButtons b){return 0x80000000u|(b==MouseButtons.Left?1u:b==MouseButtons.Right?2u:4u);}
 internal static uint[] Keyboard(bool azerty,bool mouse){return new uint[]{Key(azerty?Keys.Z:Keys.W),Key(Keys.D),Key(Keys.S),Key(azerty?Keys.Q:Keys.A),Key(Keys.NumPad9),Key(Keys.NumPad7),Key(Keys.Enter),mouse?Mouse(MouseButtons.Left):Key(Keys.NumPad1),mouse?Mouse(MouseButtons.Right):Key(Keys.NumPad2),mouse?Mouse(MouseButtons.Middle):Key(Keys.NumPad3),Key(Keys.NumPad4),Key(Keys.NumPad5),Key(Keys.NumPad6)};}
 internal static string Describe(uint code){if((code&0x80000000)!=0)return L.T("Mouse button")+" "+(code&0x7fffffff);if(code==0x40000000)return L.T("Mouse movement");if(code>=65&&code<=90||code>=48&&code<=57)return ((char)code).ToString();if(code==0x1000004)return "Enter";if(code==0x1000013)return "↑";if(code==0x1000014)return "→";if(code==0x1000015)return "↓";if(code==0x1000012)return "←";if(code>=0x1000030&&code<=0x100003b)return "F"+(code-0x1000030+1);if(code==0)return "—";return "0x"+code.ToString("X");}
 internal static void Apply(EnginePreferences engine,int type,uint[] codes){engine.Values["Input\\PerCore"]="3";engine.Values["Input\\Port\\1\\Id\\1\\Type"]=type.ToString();int start=type==227?13:0;for(int i=0;i<codes.Length;i++)engine.Values[Prefix(type)+(i+start)]=codes[i].ToString(System.Globalization.CultureInfo.InvariantCulture);}
}
internal sealed class UsbInput:IDisposable {
 [DllImport("kernel32",CharSet=CharSet.Unicode,SetLastError=true)]static extern IntPtr LoadLibraryEx(string file,IntPtr reserved,uint flags);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern int SDL_InitSubSystem(uint flags);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern void SDL_QuitSubSystem(uint flags);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern int SDL_NumJoysticks();
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern IntPtr SDL_JoystickNameForIndex(int index);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern IntPtr SDL_JoystickOpen(int index);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern void SDL_JoystickClose(IntPtr joy);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern void SDL_JoystickUpdate();
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern int SDL_JoystickNumButtons(IntPtr joy);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern int SDL_JoystickNumAxes(IntPtr joy);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern int SDL_JoystickNumHats(IntPtr joy);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern byte SDL_JoystickGetButton(IntPtr joy,int index);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern byte SDL_JoystickGetHat(IntPtr joy,int index);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern short SDL_JoystickGetAxis(IntPtr joy,int index);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern int SDL_IsGameController(int index);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern IntPtr SDL_GameControllerOpen(int index);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern void SDL_GameControllerClose(IntPtr c);
 [StructLayout(LayoutKind.Explicit,Size=12)]internal struct Binding{[FieldOffset(0)]internal int Type;[FieldOffset(4)]internal int Index;[FieldOffset(8)]internal int HatMask;}
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern Binding SDL_GameControllerGetBindForButton(IntPtr c,int button);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern Binding SDL_GameControllerGetBindForAxis(IntPtr c,int axis);
 readonly List<IntPtr> sticks=new List<IntPtr>();internal readonly List<string> Names=new List<string>();bool initialized;short[] baseline;byte[] buttons,hats;int capturing=-1;
 internal UsbInput(){var lib=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"engine","SDL2.dll");if(LoadLibraryEx(lib,IntPtr.Zero,8)==IntPtr.Zero)throw new IOException(L.T("USB input library could not be loaded."));if(SDL_InitSubSystem(0x2200)!=0)throw new IOException(L.T("USB input library could not be loaded."));initialized=true;try{for(int i=0;i<SDL_NumJoysticks();i++){sticks.Add(SDL_JoystickOpen(i));Names.Add((i+1)+" · "+Marshal.PtrToStringAnsi(SDL_JoystickNameForIndex(i)));}}catch{Dispose();throw;}}
 internal static uint Encode(int device,Binding b,bool positive=true){uint prefix=(uint)device<<18;switch(b.Type){case 1:return prefix|(uint)(b.Index+1);case 2:return prefix|(positive?0x110000u:0x100000u)|(uint)b.Index;case 3:return prefix|0x200000u|((uint)b.HatMask<<4)|(uint)b.Index;default:return 0;}}
 internal uint[] Preset(int device){if(device<0||device>=sticks.Count||SDL_IsGameController(device)==0)throw new IOException(L.T("This adapter has no automatic mapping. Assign its buttons manually."));var c=SDL_GameControllerOpen(device);if(c==IntPtr.Zero)throw new IOException(L.T("This adapter has no automatic mapping. Assign its buttons manually."));try{var b=new[]{11,14,12,13,-5,-4,6,0,1,10,2,3,9};return b.Select(id=>Encode(device,id<0?SDL_GameControllerGetBindForAxis(c,-id):SDL_GameControllerGetBindForButton(c,id))).ToArray();}finally{SDL_GameControllerClose(c);}}
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern int SDL_JoystickAttachVirtual(int type,int axes,int buttons,int hats);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern int SDL_JoystickDetachVirtual(int index);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern int SDL_JoystickSetVirtualButton(IntPtr joystick,int button,byte value);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern int SDL_JoystickSetVirtualHat(IntPtr joystick,int hat,byte value);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern int SDL_JoystickSetVirtualAxis(IntPtr joystick,int axis,short value);
 [StructLayout(LayoutKind.Sequential)]struct GuidBytes{internal ulong A,B;}
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern GuidBytes SDL_JoystickGetDeviceGUID(int index);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl)]static extern void SDL_JoystickGetGUIDString(GuidBytes guid,System.Text.StringBuilder text,int length);
 [DllImport("SDL2",CallingConvention=CallingConvention.Cdecl,CharSet=CharSet.Ansi)]static extern int SDL_GameControllerAddMapping(string mapping);
 internal static void TestVirtual(){using(var host=new UsbInput()){
  int index=SDL_JoystickAttachVirtual(1,6,15,1);if(index<0)throw new Exception("SDL virtual controller unavailable");
  try{var guid=new System.Text.StringBuilder(33);SDL_JoystickGetGUIDString(SDL_JoystickGetDeviceGUID(index),guid,33);SDL_GameControllerAddMapping(guid+",Segata test pad,a:b0,b:b1,x:b2,y:b3,start:b6,leftshoulder:b9,rightshoulder:b10,dpup:h0.1,dpright:h0.2,dpdown:h0.4,dpleft:h0.8,lefttrigger:a4,righttrigger:a5,platform:Windows,");
   using(var test=new UsbInput()){var joy=test.sticks[index];test.Begin(index);SDL_JoystickSetVirtualButton(joy,0,1);if(test.Poll()!=Encode(index,new Binding{Type=1,Index=0}))throw new Exception("USB button capture");SDL_JoystickSetVirtualButton(joy,0,0);test.Begin(index);SDL_JoystickSetVirtualHat(joy,0,2);if(test.Poll()!=Encode(index,new Binding{Type=3,Index=0,HatMask=2}))throw new Exception("USB hat capture");SDL_JoystickSetVirtualHat(joy,0,0);test.Begin(index);SDL_JoystickSetVirtualAxis(joy,0,28000);if(test.Poll()!=Encode(index,new Binding{Type=2,Index=0}))throw new Exception("USB axis capture");var mapped=test.Preset(index);if(mapped.Length!=13||mapped.Any(c=>c==0)||mapped[7]!=Encode(index,new Binding{Type=1,Index=0}))throw new Exception("SDL preset binding ABI");}
  }finally{SDL_JoystickDetachVirtual(index);}
 }}
 internal void Begin(int device){capturing=device;if(device<0||device>=sticks.Count||sticks[device]==IntPtr.Zero){capturing=-1;return;}SDL_JoystickUpdate();var j=sticks[device];baseline=Enumerable.Range(0,SDL_JoystickNumAxes(j)).Select(i=>SDL_JoystickGetAxis(j,i)).ToArray();buttons=Enumerable.Range(0,SDL_JoystickNumButtons(j)).Select(i=>SDL_JoystickGetButton(j,i)).ToArray();hats=Enumerable.Range(0,SDL_JoystickNumHats(j)).Select(i=>SDL_JoystickGetHat(j,i)).ToArray();}
 internal uint Poll(){if(capturing<0)return 0;SDL_JoystickUpdate();var j=sticks[capturing];for(int i=0;i<buttons.Length;i++){byte b=SDL_JoystickGetButton(j,i);bool press=b!=0&&buttons[i]==0;buttons[i]=b;if(press)return Encode(capturing,new Binding{Type=1,Index=i});}for(int i=0;i<hats.Length;i++){byte h=SDL_JoystickGetHat(j,i),old=hats[i];hats[i]=h;foreach(int bit in new[]{1,2,4,8})if((h&bit)!=0&&(old&bit)==0)return Encode(capturing,new Binding{Type=3,Index=i,HatMask=bit});}for(int i=0;i<baseline.Length;i++){int delta=SDL_JoystickGetAxis(j,i)-baseline[i];if(Math.Abs(delta)>20000)return Encode(capturing,new Binding{Type=2,Index=i},delta>0);}return 0;}
 public void Dispose(){foreach(var j in sticks)if(j!=IntPtr.Zero)SDL_JoystickClose(j);sticks.Clear();if(initialized){SDL_QuitSubSystem(0x2200);initialized=false;}}
}
internal sealed class InputCapture:Form {
 internal uint Code{get;private set;}readonly Timer timer=new Timer{Interval=25};
 internal InputCapture(UsbInput input,int device,bool movement){Text=L.T("Assign input");Width=520;Height=200;StartPosition=FormStartPosition.CenterParent;KeyPreview=true;BackColor=System.Drawing.Color.FromArgb(35,37,33);ForeColor=System.Drawing.Color.White;
  Controls.Add(new Label{Text=L.T("Press a key, mouse button or controller button. Escape cancels."),Dock=DockStyle.Fill,Padding=new Padding(20)});if(input!=null)input.Begin(device);
  timer.Tick+=(s,e)=>{if(input!=null){uint code=input.Poll();if(code!=0)Finish(code);}};Shown+=(s,e)=>timer.Start();FormClosed+=(s,e)=>timer.Dispose();
  MouseDown+=(s,e)=>Finish(ControllerInput.Mouse(e.Button));Controls[0].MouseDown+=(s,e)=>Finish(ControllerInput.Mouse(e.Button));
  if(movement){MouseMove+=(s,e)=>Finish(0x40000000);Controls[0].MouseMove+=(s,e)=>Finish(0x40000000);}
 }
 protected override bool ProcessCmdKey(ref Message msg,Keys data){var key=data&Keys.KeyCode;if(key==Keys.Escape){Close();return true;}var code=ControllerInput.Key(key);if(code!=0){Finish(code);return true;}return base.ProcessCmdKey(ref msg,data);}
 void Finish(uint code){Code=code;DialogResult=DialogResult.OK;Close();}
}
}
