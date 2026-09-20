using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
namespace SaturnApp {
internal static class RegionalBios {
 internal static string Detect(string game,string preferred){
  string file=game;try{
   if(Path.GetExtension(file).Equals(".chd",StringComparison.OrdinalIgnoreCase))return preferred;
   if(Path.GetExtension(file).Equals(".cue",StringComparison.OrdinalIgnoreCase)){var m=Regex.Match(File.ReadAllText(file),"^\\s*FILE\\s+(?:\"([^\"]+)\"|(\\S+))",RegexOptions.Multiline|RegexOptions.IgnoreCase);if(!m.Success)return preferred;file=Path.Combine(Path.GetDirectoryName(file),m.Groups[1].Success?m.Groups[1].Value:m.Groups[2].Value);}
   using(var stream=File.OpenRead(file)){var buffer=new byte[65536];int length=stream.Read(buffer,0,buffer.Length);string text=Encoding.ASCII.GetString(buffer,0,length);int head=text.IndexOf("SEGA SEGASATURN",StringComparison.Ordinal);if(head<0||head+80>length)return preferred;string area=text.Substring(head+64,16);char preferredCode=preferred=="US"?'U':preferred=="EU"?'E':'J';if(area.IndexOf(preferredCode)>=0)return preferred;if(area.IndexOf('J')>=0)return "JP";if(area.IndexOf('U')>=0)return "US";if(area.IndexOf('E')>=0)return "EU";}
  }catch(IOException){}catch(UnauthorizedAccessException){}return preferred;
 }
 internal static string Select(Settings s,string game){if(s.Hle)return "";string region=s.BiosRegion=="auto"?Detect(game,s.DefaultRegion):s.BiosRegion;string bios=region=="US"?s.BiosUsa:region=="EU"?s.BiosEurope:s.BiosJapan;
  if(string.IsNullOrWhiteSpace(bios)&&s.BiosRegion=="auto"&&string.IsNullOrWhiteSpace(s.BiosJapan+s.BiosUsa+s.BiosEurope))bios=s.Bios;
  if(!File.Exists(bios))throw new IOException(L.T("Select the BIOS for this region in Settings.")+" ("+region+")");return bios;
 }
 internal static bool HasBios(Settings s){return s.Hle||File.Exists(s.Bios)||File.Exists(s.BiosJapan)||File.Exists(s.BiosUsa)||File.Exists(s.BiosEurope);}
}
}
