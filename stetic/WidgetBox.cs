using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic {

	public class WidgetBox : Gtk.Bin {

		static Color black, white;

		static WidgetBox ()
		{
			black = new Color (0, 0, 0);
			black.Pixel = 1;
			white = new Color (255, 255, 255);
			white.Pixel = 0;
		}

		public WidgetBox ()
		{
			WidgetFlags &= ~WidgetFlags.NoWindow;
			ShowPlaceholder = true;
		}

		bool showPlaceholder;
		protected bool ShowPlaceholder {
			get { return showPlaceholder; }
			set {
				if (value == showPlaceholder)
					return;
				showPlaceholder = value;

				if (IsRealized) {
					bool wasMapped = IsMapped;

					if (wasMapped)
						Hide ();
					Unrealize ();
					Realize ();
					if (wasMapped)
						Show ();
				}
			}
		}

		Gdk.Window HandleWindow;
		bool showHandles;
		protected bool ShowHandles {
			get { return showHandles; }
			set {
				if (value == showHandles)
					return;
				showHandles = value;

				if (showHandles) {
					if (IsRealized) {
						HandleWindow = NewWindow (Toplevel.GdkWindow, Gdk.WindowClass.InputOutput);
						HandleWindow.Background = black;
						ShapeHandles ();
					}
					if (IsMapped)
						HandleWindow.Show ();
				} else {
					if (HandleWindow != null) {
						HandleWindow.Hide ();
						HandleWindow.Destroy ();
						HandleWindow = null;
					}
				}
			}
		}

		Gdk.Window EventWindow;
		bool interceptEvents;
		protected bool InterceptEvents {
			get { return interceptEvents; }
			set {
				if (value == interceptEvents)
					return;
				interceptEvents = value;

				if (interceptEvents) {
					if (IsRealized)
						EventWindow = NewWindow (ParentWindow, Gdk.WindowClass.InputOnly);
					if (IsMapped)
						EventWindow.Show ();
				} else {
					if (EventWindow != null) {
						EventWindow.Hide ();
						EventWindow.Destroy ();
						EventWindow = null;
					}
				}
			}
		}

		protected override void OnSizeRequested (ref Requisition req)
		{
			if (Child == null)
				req.Width = req.Height = 0;
			else
				req = Child.SizeRequest ();
		}

		protected override void OnSizeAllocated (Rectangle allocation)
		{
			Allocation = allocation;

			if (EventWindow != null)
				EventWindow.MoveResize (allocation);
			if (GdkWindow != null && GdkWindow != ParentWindow)
				GdkWindow.MoveResize (allocation);
			if (HandleWindow != null)
				ShapeHandles ();

			if (Child != null) {
				allocation.X = allocation.Y = 0;
				Child.SizeAllocate (allocation);
			}
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

		Gdk.Window NewWindow (Gdk.Window parent, Gdk.WindowClass wclass)
		{
			WindowAttr attributes;
			WindowAttributesType attributesMask;
			Gdk.Window win;

			attributes = new WindowAttr ();
			attributes.WindowType = Gdk.WindowType.Child;
			attributes.X = Allocation.X;
			attributes.Y = Allocation.Y;
			attributes.Width = Allocation.Width;
			attributes.Height = Allocation.Height;
			attributes.Wclass = wclass ;
			attributes.visual = Visual;
			attributes.colormap = Colormap;
			attributes.EventMask = (int)(Events |
						     EventMask.ButtonPressMask |
						     EventMask.ButtonMotionMask |
						     EventMask.ButtonReleaseMask |
						     EventMask.ExposureMask |
						     EventMask.EnterNotifyMask |
						     EventMask.LeaveNotifyMask);

			attributesMask =
				Gdk.WindowAttributesType.X |
				Gdk.WindowAttributesType.Y |
				Gdk.WindowAttributesType.Visual |
				Gdk.WindowAttributesType.Colormap;

			win = new Gdk.Window (parent, attributes, attributesMask);
			win.UserData = Handle;

			if (wclass == Gdk.WindowClass.InputOutput)
				Style.Attach (win);

			return win;
		}

		protected override void OnRealized ()
		{
			WidgetFlags |= WidgetFlags.Realized;

			GdkWindow = NewWindow (ParentWindow, Gdk.WindowClass.InputOutput);

			if (ShowPlaceholder) {
				Gdk.Pixmap pixmap, mask;
				pixmap = Gdk.Pixmap.CreateFromXpmD (GdkWindow, out mask, new Gdk.Color (99, 99, 99), placeholder_xpm);
				GdkWindow.SetBackPixmap (pixmap, false);
			} else
				Style.SetBackground (GdkWindow, StateType.Normal);

			if (InterceptEvents)
				EventWindow = NewWindow (ParentWindow, Gdk.WindowClass.InputOnly);

			if (ShowHandles) {
				HandleWindow = NewWindow (Toplevel.GdkWindow, Gdk.WindowClass.InputOutput);
				HandleWindow.Background = black;
				ShapeHandles ();
			}
		}

		private const int handleSize = 6;
		void ShapeHandles ()
		{
			Gdk.GC gc;
			Gdk.Pixmap pixmap;
			Rectangle handleAllocation;
			int tlx, tly;

			TranslateCoordinates (Toplevel, 0, 0, out tlx, out tly);
			handleAllocation.X = tlx - handleSize / 2;
			handleAllocation.Y = tly - handleSize / 2;
			handleAllocation.Width = Allocation.Width + handleSize;
			handleAllocation.Height = Allocation.Height + handleSize;

			int width = handleAllocation.Width, height = handleAllocation.Height;

			pixmap = new Pixmap (HandleWindow, width, height, 1);
			gc = new Gdk.GC (pixmap);
			gc.Background = white;
			gc.Foreground = white;
			pixmap.DrawRectangle (gc, true, 0, 0, width, height);

			gc.Foreground = black;

			// Draw border
			pixmap.DrawRectangle (gc, false, handleSize / 2, handleSize / 2,
					      width - handleSize, height - handleSize);

			if (!ShowPlaceholder) {
				// Draw corner handles
				pixmap.DrawRectangle (gc, true, 0, 0, handleSize, handleSize);
				pixmap.DrawRectangle (gc, true, 0, height - handleSize, handleSize, handleSize);
				pixmap.DrawRectangle (gc, true, width - handleSize, 0, handleSize, handleSize);
				pixmap.DrawRectangle (gc, true, width - handleSize, height - handleSize, handleSize, handleSize);
			}

			HandleWindow.MoveResize (handleAllocation);
			HandleWindow.ShapeCombineMask (pixmap, 0, 0);
		}

		protected override void OnMapped ()
		{
			base.OnMapped ();
			if (EventWindow != null)
				EventWindow.Show ();
			if (HandleWindow != null)
				HandleWindow.Show ();
		}

		protected override void OnUnmapped ()
		{
			if (EventWindow != null)
				EventWindow.Hide ();
			if (HandleWindow != null)
				HandleWindow.Hide ();
			base.OnUnmapped ();
		}

		protected override void OnUnrealized ()
		{
			if (EventWindow != null) {
				EventWindow.Destroy ();
				EventWindow = null;
			}
			if (HandleWindow != null) {
				HandleWindow.Destroy ();
				HandleWindow = null;
			}
			base.OnUnrealized ();
		}

		protected override bool OnExposeEvent (Gdk.EventExpose evt)
		{
			if (!IsDrawable)
				return false;

			if (ShowPlaceholder) {
				int width, height;
				GdkWindow.GetSize (out width, out height);

				Gdk.GC light, dark;
				light = Style.LightGC (StateType.Normal);
				dark = Style.DarkGC (StateType.Normal);

				GdkWindow.DrawLine (light, 0, 0, width - 1, 0);
				GdkWindow.DrawLine (light, 0, 0, 0, height - 1);
				GdkWindow.DrawLine (dark, 0, height - 1, width - 1, height - 1);
				GdkWindow.DrawLine (dark, width - 1, 0, width - 1, height - 1);
			}

			return base.OnExposeEvent (evt);
		}
	}
}
