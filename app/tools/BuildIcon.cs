using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
class BuildIcon {
 static void Main(string[] args){
  var frames=new List<byte[]>();var sizes=new[]{16,24,32,48,64,128,256};
  using(var source=new Bitmap(args[0]))foreach(int n in sizes){
   using(var bitmap=new Bitmap(n,n,System.Drawing.Imaging.PixelFormat.Format32bppArgb)){
    // Encode the existing artwork at standard ICO sizes, retaining pixel edges.
    int side=Math.Max(source.Width,source.Height);double scale=(n-2.0)/side;
    for(int y=0;y<n;y++)for(int x=0;x<n;x++){
     int sx=(int)Math.Floor((x-(n-source.Width*scale)/2)/scale),sy=(int)Math.Floor((y-(n-source.Height*scale)/2)/scale);
     bitmap.SetPixel(x,y,sx>=0&&sx<source.Width&&sy>=0&&sy<source.Height?source.GetPixel(sx,sy):Color.Transparent);
    }
    using(var stream=new MemoryStream())using(var w=new BinaryWriter(stream)){
     int stride=((n+31)/32)*4;w.Write(40);w.Write(n);w.Write(n*2);w.Write((ushort)1);w.Write((ushort)32);w.Write(0);w.Write(n*n*4+stride*n);w.Write(0);w.Write(0);w.Write(0);w.Write(0);
     for(int y=n-1;y>=0;y--)for(int x=0;x<n;x++){var c=bitmap.GetPixel(x,y);w.Write(c.B);w.Write(c.G);w.Write(c.R);w.Write(c.A);}
     for(int y=n-1;y>=0;y--){var mask=new byte[stride];for(int x=0;x<n;x++)if(bitmap.GetPixel(x,y).A==0)mask[x/8]|=(byte)(128>>(x%8));w.Write(mask);}frames.Add(stream.ToArray());
    }
   }
  }
  using(var w=new BinaryWriter(File.Create(args[1]))){w.Write((ushort)0);w.Write((ushort)1);w.Write((ushort)sizes.Length);int offset=6+16*sizes.Length;for(int i=0;i<sizes.Length;i++){w.Write((byte)(sizes[i]==256?0:sizes[i]));w.Write((byte)(sizes[i]==256?0:sizes[i]));w.Write((byte)0);w.Write((byte)0);w.Write((ushort)1);w.Write((ushort)32);w.Write(frames[i].Length);w.Write(offset);offset+=frames[i].Length;}foreach(var bytes in frames)w.Write(bytes);}
  foreach(int n in sizes)using(var icon=new Icon(args[1],n,n))using(var bmp=icon.ToBitmap()){int visible=0;for(int y=0;y<bmp.Height;y++)for(int x=0;x<bmp.Width;x++)if(bmp.GetPixel(x,y).A>0)visible++;if(visible<n*n/10)throw new Exception("Transparent icon at "+n);Console.WriteLine(n+"px: "+visible+" visible pixels");if(n==128)bmp.Save(args[2]);}
 }
}
