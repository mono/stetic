using Gtk;
using System;

namespace Stetic {

	public class Placeholder : WidgetBox {

		public Placeholder ()
		{
			DND.DestSet (this, true);
			Internal = true;
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

		protected override void OnRealized ()
		{
			WidgetFlags |= WidgetFlags.Realized;

			GdkWindow = NewWindow (ParentWindow, Gdk.WindowClass.InputOutput);

			Gdk.Pixmap pixmap, mask;
			pixmap = Gdk.Pixmap.CreateFromXpmD (GdkWindow, out mask, new Gdk.Color (99, 99, 99), placeholder_xpm);
			GdkWindow.SetBackPixmap (pixmap, false);
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

			GdkWindow.DrawLine (light, 0, 0, width - 1, 0);
			GdkWindow.DrawLine (light, 0, 0, 0, height - 1);
			GdkWindow.DrawLine (dark, 0, height - 1, width - 1, height - 1);
			GdkWindow.DrawLine (dark, width - 1, 0, width - 1, height - 1);

			return base.OnExposeEvent (evt);
		}

		public delegate void DropHandler (Placeholder ph, Widget w);
		public event DropHandler Drop;

		protected override bool OnDragDrop (Gdk.DragContext ctx, int x, int y, uint time)
		{
			Widget dragged = DND.Drop (ctx, time);
			if (dragged == null)
				return false;

			if (Drop != null)
				Drop (this, dragged);
			else
				dragged.Destroy ();
			return true;
		}

		public void Mimic (WidgetSite site)
		{
			Gdk.Rectangle alloc = site.Allocation;
			SetSizeRequest (alloc.Width, alloc.Height);
			hexpandable = site.HExpandable;
			vexpandable = site.VExpandable;
		}

		public void UnMimic ()
		{
			SetSizeRequest (-1, -1);
			hexpandable = vexpandable = true;
		}

		bool hexpandable = true;
		public override bool HExpandable {
			get {
				return hexpandable;
			}
		}

		bool vexpandable = true;
		public override bool VExpandable {
			get {
				return vexpandable;
			}
		}
	}
}
