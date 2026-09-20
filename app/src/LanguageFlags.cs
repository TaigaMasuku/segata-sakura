using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
namespace SaturnApp {
// Small code-drawn flag badges, independent of Windows emoji support.
internal static class LanguageFlags {
 internal static void Draw(Graphics g,Rectangle r,string code){var state=g.Save();g.SetClip(r);g.TranslateTransform(r.X,r.Y);g.ScaleTransform(r.Width/30f,r.Height/20f);
  if(code=="fr"){g.FillRectangle(Brushes.White,0,0,30,20);g.FillRectangle(Brushes.RoyalBlue,0,0,10,20);g.FillRectangle(Brushes.Crimson,20,0,10,20);}
  else if(code=="es"){g.FillRectangle(Brushes.Crimson,0,0,30,20);g.FillRectangle(Brushes.Gold,0,5,30,10);g.FillRectangle(Brushes.DarkRed,7,8,3,5);}
  else if(code=="pt-PT"){g.FillRectangle(Brushes.Crimson,0,0,30,20);g.FillRectangle(Brushes.DarkGreen,0,0,12,20);g.FillEllipse(Brushes.Gold,8,5,8,10);g.FillRectangle(Brushes.White,10,7,4,6);}
  else if(code=="pt-BR"){g.FillRectangle(Brushes.ForestGreen,0,0,30,20);g.FillPolygon(Brushes.Gold,new[]{new Point(15,2),new Point(28,10),new Point(15,18),new Point(2,10)});g.FillEllipse(Brushes.RoyalBlue,10,5,10,10);using(var pen=new Pen(Color.White,1.3f))g.DrawLine(pen,10,9,20,11);}
  else{g.FillRectangle(Brushes.DarkBlue,0,0,30,20);using(var p=new Pen(Color.White,5)){g.DrawLine(p,0,0,30,20);g.DrawLine(p,0,20,30,0);}using(var p=new Pen(Color.Crimson,2)){g.DrawLine(p,0,0,30,20);g.DrawLine(p,0,20,30,0);}g.FillRectangle(Brushes.White,12,0,6,20);g.FillRectangle(Brushes.White,0,7,30,6);g.FillRectangle(Brushes.Crimson,13,0,4,20);g.FillRectangle(Brushes.Crimson,0,8,30,4);}
  g.Restore(state);
 }
 internal static void PaintItem(ComboBox c,DrawItemEventArgs e,bool flags){using(var brush=new SolidBrush(Color.FromArgb(48,50,44)))e.Graphics.FillRectangle(brush,e.Bounds);if(e.Index>=0){var text=e.Bounds;if(flags){Draw(e.Graphics,new Rectangle(text.X+6,text.Y+4,27,18),L.Codes[e.Index]);text.X+=42;text.Width-=42;}TextRenderer.DrawText(e.Graphics,c.Items[e.Index].ToString(),c.Font,text,Color.FromArgb(244,241,231),TextFormatFlags.VerticalCenter|TextFormatFlags.Left);}e.DrawFocusRectangle();}
 internal static void Attach(ComboBox c){c.DrawMode=DrawMode.OwnerDrawFixed;c.ItemHeight=27;c.DrawItem+=(s,e)=>PaintItem(c,e,true);}
}
}
