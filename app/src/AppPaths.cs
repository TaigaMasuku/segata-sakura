using System;
using System.IO;
namespace SaturnApp {
internal static class AppPaths {
 public static string Data {
  get {var test=Environment.GetEnvironmentVariable("SEGATA_SAKURA_DATA");if(!string.IsNullOrEmpty(test))return Path.GetFullPath(test);
   var local=Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
   if(string.IsNullOrEmpty(local))local=Environment.GetEnvironmentVariable("LOCALAPPDATA");
   if(string.IsNullOrEmpty(local))throw new IOException("Le dossier de données Windows est introuvable.");
   return Path.Combine(local,"SegataSakura");}
 }
 public static string Initialize(string root){var data=Data;Directory.CreateDirectory(data);var old=Path.Combine(root,"settings.xml");var dest=Path.Combine(data,"settings.xml");if(!File.Exists(dest)&&File.Exists(old))File.Copy(old,dest);var oldCovers=Path.Combine(root,"covers");if(Directory.Exists(oldCovers)){var covers=Path.Combine(data,"covers");Directory.CreateDirectory(covers);foreach(var f in Directory.GetFiles(oldCovers,"*.png")){var target=Path.Combine(covers,Path.GetFileName(f));if(!File.Exists(target))File.Copy(f,target);}}return data;}
}
}
