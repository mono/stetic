using System;
using System.Runtime.InteropServices;

namespace Stetic {

	// This ought to be a subclass of Gdk.Window, but GdkWindow isn't subclassable

	public class HandleWindow : IDisposable {

		static Gdk.Color black, white;

		static HandleWindow ()
		{
			black = new Gdk.Color (0, 0, 0);
			black.Pixel = 1;
			white = new Gdk.Color (255, 255, 255);
			white.Pixel = 0;
		}

		public HandleWindow (Gtk.Widget selection, bool dragHandles)
		{
			this.selection = selection;
			this.dragHandles = dragHandles;

			selection.SizeAllocated += SelectionResized;

			invis = new Gtk.Invisible ();
			invis.WidgetEvent += HandleEvent;

			Gdk.WindowAttr attributes = new Gdk.WindowAttr ();
			attributes.WindowType = Gdk.WindowType.Child;
			attributes.Wclass = Gdk.WindowClass.InputOutput;
			attributes.Visual = selection.Visual;
			attributes.Colormap = selection.Colormap;
			attributes.Mask = (Gdk.EventMask.ButtonPressMask |
					   Gdk.EventMask.ButtonMotionMask |
					   Gdk.EventMask.ButtonReleaseMask |
					   Gdk.EventMask.ExposureMask);
			window = new Gdk.Window (selection.Toplevel.GdkWindow, attributes,
						 Gdk.WindowAttributesType.Visual |
						 Gdk.WindowAttributesType.Colormap);
			window.UserData = invis.Handle;
			selection.Style.Attach (window);

			Shape ();
		}

		public void Dispose ()
		{
			if (selection != null) {
				selection.SizeAllocated -= SelectionResized;
				selection = null;
			}

			if (invis != null) {
				invis.Destroy ();
				invis = null;
			}

			if (window != null) {
				window.Destroy ();
				window = null;
			}
		}

		Gtk.Widget selection, invis;
		bool dragHandles;

		Gdk.Window window;
		public Gdk.Window Window {
			get {
				return window;
			}
		}

		Gdk.Rectangle handleAllocation;

		const int handleSize = 6;

		public void Shape ()
		{
			Gdk.GC gc;
			Gdk.Pixmap pixmap;
			int tlx, tly;

			selection.TranslateCoordinates (selection.Toplevel, 0, 0, out tlx, out tly);
			handleAllocation.X = tlx - handleSize / 2;
			handleAllocation.Y = tly - handleSize / 2;
			handleAllocation.Width = selection.Allocation.Width + handleSize;
			handleAllocation.Height = selection.Allocation.Height + handleSize;

			int width = handleAllocation.Width, height = handleAllocation.Height;

			pixmap = new Gdk.Pixmap (window, width, height, 1);
			gc = new Gdk.GC (pixmap);
			gc.Background = white;
			gc.Foreground = white;
			pixmap.DrawRectangle (gc, true, 0, 0, width, height);

			gc.Foreground = black;

			// Draw border
			pixmap.DrawRectangle (gc, false, handleSize / 2, handleSize / 2,
					      width - handleSize, height - handleSize);

			if (dragHandles) {
				pixmap.DrawRectangle (gc, true, 0, 0, handleSize, handleSize);
				pixmap.DrawRectangle (gc, true, 0, height - handleSize, handleSize, handleSize);
				pixmap.DrawRectangle (gc, true, width - handleSize, 0, handleSize, handleSize);
				pixmap.DrawRectangle (gc, true, width - handleSize, height - handleSize, handleSize, handleSize);
			}

			window.Hide ();
			window.MoveResize (handleAllocation);
			window.ShapeCombineMask (pixmap, 0, 0);
			window.Show ();
		}

		void SelectionResized (object obj, Gtk.SizeAllocatedArgs args)
		{
			Shape ();
		}

		int clickX, clickY;

		[GLib.ConnectBefore]
		void HandleEvent (object obj, Gtk.WidgetEventArgs args)
		{
			args.RetVal = true;

			switch (args.Event.Type) {
			case Gdk.EventType.ButtonPress:
				Gdk.EventButton evb = (Gdk.EventButton)args.Event;

				if (evb.Type == Gdk.EventType.ButtonPress && evb.Button == 1) {
					clickX = (int)evb.XRoot;
					clickY = (int)evb.YRoot;
				}
				return;

			case Gdk.EventType.MotionNotify:
				Gdk.EventMotion evm = (Gdk.EventMotion)args.Event;

				if (!dragHandles)
					return;
				if ((evm.State & Gdk.ModifierType.Button1Mask) == 0)
					return;
				if (!Gtk.Drag.CheckThreshold (selection, clickX, clickY, (int)evm.XRoot, (int)evm.YRoot))
					return;

				if (Drag != null)
					Drag (evm);
				return;

			case Gdk.EventType.Expose:
				// hack around bgo 316871 for gtk+ 2.8.0-2.8.3
				gdk_synthesize_window_state (window.Handle, 0, 1);
				selection.Toplevel.GdkWindow.InvalidateRect (handleAllocation, true);
				gdk_synthesize_window_state (window.Handle, 1, 0);
				return;

			default:
				return;
			}
		}

		public delegate void DragDelegate (Gdk.EventMotion evt);
		public event DragDelegate Drag;


		[DllImport ("libgdk-win32-2.0-0.dll")]
		static extern void gdk_synthesize_window_state (IntPtr window, uint unset_flags, uint set_flags);
	}
}

