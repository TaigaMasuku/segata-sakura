using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
namespace SaturnApp {
// Keys/IDs verified against Kronos 2.7.0 UISettings.cpp and YabauseThread.cpp.
internal sealed class EnginePreferences {
 internal static readonly Dictionary<string,string> Defaults=new Dictionary<string,string>{
  {"Video\\filter_type","0"},{"Video\\upscale_type","0"},{"Video\\resolution_mode","1"},{"Video\\AspectRatio","0"},
  {"General\\EnableVSync","true"},{"General\\ShowFPS","false"},{"Sound\\Volume","100"},{"Video\\Fullscreen","false"}};
 readonly string path;readonly Dictionary<string,string> original;
 internal readonly Dictionary<string,string> Values;
 internal EnginePreferences(string engine){path=Path.Combine(Path.GetDirectoryName(engine),"kronos.ini");original=Read(path);Values=new Dictionary<string,string>(original);foreach(var p in Defaults)Values[p.Key]=original.ContainsKey(p.Key)?original[p.Key]:p.Value;}
 internal static Dictionary<string,string> Read(string path){var result=new Dictionary<string,string>();if(!File.Exists(path))return result;string section="";foreach(var line in File.ReadAllLines(path)){string t=line.Trim();if(t.StartsWith("[")){section=t;continue;}int equal=line.IndexOf('=');if(section=="[1.0]"&&equal>0)result[line.Substring(0,equal).Trim()]=line.Substring(equal+1).Trim();}return result;}
 internal static void EnsureStopped(){foreach(var p in Process.GetProcessesByName("kronos")){using(p){if(!p.HasExited)throw new IOException(L.T("Close Kronos before continuing."));}}}
 internal void Save(Settings settings,string settingsPath){
  EnsureStopped();var changed=Values.Where(p=>p.Value!=(original.ContainsKey(p.Key)?original[p.Key]:(Defaults.ContainsKey(p.Key)?Defaults[p.Key]:null))).ToArray();
  if(changed.Length==0){settings.Save(settingsPath);return;}
  if(!Directory.Exists(Path.GetDirectoryName(path)))throw new IOException(L.T("Kronos executable was not found. Reinstall the application."));
  var current=Read(path);foreach(var p in changed){string fallback=Defaults.ContainsKey(p.Key)?Defaults[p.Key]:null;string a=original.ContainsKey(p.Key)?original[p.Key]:fallback,b=current.ContainsKey(p.Key)?current[p.Key]:fallback;if(a!=b)throw new IOException(L.T("Kronos settings changed. Close and reopen Settings before saving."));}
  bool existed=File.Exists(path);byte[] before=existed?File.ReadAllBytes(path):null;
  var lines=existed?File.ReadAllLines(path).ToList():new List<string>();int begin=lines.FindIndex(x=>x.Trim()=="[1.0]");if(begin<0){lines.Add("[1.0]");begin=lines.Count-1;}int end=begin+1;while(end<lines.Count&&!lines[end].TrimStart().StartsWith("["))end++;
  foreach(var pair in changed){bool found=false;for(int i=begin+1;i<end;i++){int equal=lines[i].IndexOf('=');if(equal>0&&lines[i].Substring(0,equal).Trim()==pair.Key){lines[i]=pair.Key+"="+pair.Value;found=true;}}if(!found){lines.Insert(end,pair.Key+"="+pair.Value);end++;}}
  string tmp=path+".segata.tmp";File.WriteAllLines(tmp,lines,new UTF8Encoding(false));if(existed)File.Replace(tmp,path,path+".segata.bak");else File.Move(tmp,path);
  try{settings.Save(settingsPath);}catch{if(existed)File.WriteAllBytes(path,before);else File.Delete(path);throw;}
 }
}
}
