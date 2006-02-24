using Gtk;
using System;

namespace Stetic {

	public class Placeholder : Gtk.DrawingArea {

		public Placeholder ()
		{
			DND.DestSet (this, true);
			Events |= Gdk.EventMask.ButtonPressMask;
		}

		const int minSize = 10;

		protected override void OnSizeRequested (ref Requisition req)
		{
			base.OnSizeRequested (ref req);
			if (req.Width <= 0)
				req.Width = minSize;
			if (req.Height <= 0)
				req.Height = minSize;
		}

		static private string[] placeholder_xpm = {
			"8 8 2 1",
			"  c #bbbbbb",
			". c #d6d6d6",
			"   ..   ",
			"  .  .  ",
			" .    . ",
			".      .",
			".      .",
			" .    . ",
			"  .  .  ",
			"   ..   "
		};

		Gdk.Pixmap pixmap;
		
		protected override void OnRealized ()
		{
			base.OnRealized ();

			Gdk.Pixmap mask;
			pixmap = Gdk.Pixmap.CreateFromXpmD (GdkWindow, out mask, new Gdk.Color (99, 99, 99), placeholder_xpm);
		}

		protected override bool OnExposeEvent (Gdk.EventExpose evt)
		{
			if (!IsDrawable)
				return false;

			int width, height;
			GdkWindow.GetSize (out width, out height);

			Gdk.GC light, dark;
			light = Style.LightGC (StateType.Normal);
			dark = Style.DarkGC (StateType.Normal);

			// Looks like GdkWindow.SetBackPixmap doesn't work very well,
			// so draw the pixmap manually.
			light.Fill = Gdk.Fill.Tiled;
			light.Tile = pixmap;
			GdkWindow.DrawRectangle (light, true, 0, 0, width, height);
			light.Fill = Gdk.Fill.Solid;

			GdkWindow.DrawLine (light, 0, 0, width - 1, 0);
			GdkWindow.DrawLine (light, 0, 0, 0, height - 1);
			GdkWindow.DrawLine (dark, 0, height - 1, width - 1, height - 1);
			GdkWindow.DrawLine (dark, width - 1, 0, width - 1, height - 1);

			return base.OnExposeEvent (evt);
		}
	}
}
