using System;

namespace Stetic {

	public static class DND {
		static DND ()
		{
			SteticWidgetType = Gdk.Atom.Intern ("application/x-stetic-widget", false);

			Targets = new Gtk.TargetEntry[1];
			Targets[0] = new Gtk.TargetEntry ("application/x-stetic-widget", 0, 0);

			TargetList = new Gtk.TargetList ();
			TargetList.Add (SteticWidgetType, 0, 0);
		}

		public static Gtk.TargetEntry[] Targets;
		public static Gtk.TargetList TargetList;
		public static Gdk.Atom SteticWidgetType;

		public static void SourceSet (Gtk.Widget source, bool automatic)
		{
			if (automatic) {
				Gtk.Drag.SourceSet (source, Gdk.ModifierType.Button1Mask,
						    Targets, Gdk.DragAction.Move);
			} else
				source.ButtonPressEvent += SourceButtonPress;
		}

		public static void SourceUnset (Gtk.Widget source)
		{
			Gtk.Drag.SourceUnset (source);
			source.ButtonPressEvent -= SourceButtonPress;
		}

		public static void DestSet (Gtk.Widget dest, bool automatic)
		{
			Gtk.Drag.DestSet (dest, automatic ? Gtk.DestDefaults.All : 0,
					  Targets, Gdk.DragAction.Move);
		}

		public static void DestUnset (Gtk.Widget dest)
		{
			Gtk.Drag.DestUnset (dest);
		}

		static Gtk.Widget dragWidget;
		static int clickX, clickY;

		[GLib.ConnectBefore]
		static void SourceButtonPress (object obj, Gtk.ButtonPressEventArgs args)
		{
			Gdk.EventButton evt = args.Event;
			if (evt.Button == 1 && evt.Type == Gdk.EventType.ButtonPress) {
				dragWidget = obj as Gtk.Widget;
				clickX = (int)evt.XRoot;
				clickY = (int)evt.YRoot;
			}
		}

		// Non-automatic drag sources should call this on any MotionEvent
		// to see if that motion can start a drag
		public static bool CanDrag (Gtk.Widget source, Gdk.EventMotion evt)
		{
			if ((evt.State & Gdk.ModifierType.Button1Mask) == 0)
				return false;
			if (source != dragWidget)
				return false;
			return Gtk.Drag.CheckThreshold (source, clickX, clickY, (int)evt.XRoot, (int)evt.YRoot);
		}

		// Drag function for non-automatic sources, called from MotionNotifyEvent
		public static void Drag (Gtk.Widget source, Gdk.EventMotion evt, Gtk.Widget dragWidget)
		{
			Gdk.DragContext ctx;

			ctx = Gtk.Drag.Begin (source, TargetList, Gdk.DragAction.Move,
					      1 /* button */, evt);
			Drag (source, ctx, dragWidget);
		}

		// Drag function for automatic sources, called from DragBegin
		public static void Drag (Gtk.Widget source, Gdk.DragContext ctx, Gtk.Widget dragWidget)
		{
			Gtk.Window dragWin;
			Gtk.Requisition req;

			if (DragBegin != null)
				DragBegin ();

			DND.dragWidget = dragWidget;

			dragWin = new Gtk.Window (Gtk.WindowType.Popup);
			dragWin.Add (dragWidget);

			req = dragWidget.SizeRequest ();
			if (req.Width < 20 && req.Height < 20)
				dragWin.SetSizeRequest (20, 20);
			else if (req.Width < 20)
				dragWin.SetSizeRequest (20, -1);
			else if (req.Height < 20)
				dragWin.SetSizeRequest (-1, 20);

			int px, py, rx, ry;
			Gdk.ModifierType pmask;
			ctx.SourceWindow.GetPointer (out px, out py, out pmask);
			ctx.SourceWindow.GetRootOrigin (out rx, out ry);

			dragWin.Move (rx + px, ry + py);
			dragWin.Show ();
			Gtk.Drag.SetIconWidget (ctx, dragWin, 0, 0);

			source.DragEnd += DragEnded;
		}

		public static Gtk.Widget DragWidget {
			get {
				return dragWidget;
			}
		}

		// Call this from a DragDrop event to receive the dragged widget
		public static Gtk.Widget Drop (Gdk.DragContext ctx, uint time)
		{
			if (dragWidget == null) {
				// This would only happen if you dragged from another
				// process. Maybe in the future we could handle that by
				// serializing to glade format. But not yet.
				Gtk.Drag.Finish (ctx, false, false, time);
				return null;
			}

			Gtk.Widget w = dragWidget;
			dragWidget = null;

			// Remove the widget from its dragWindow
			Gtk.Container parent = w.Parent as Gtk.Container;
			if (parent != null)
				parent.Remove (w);

			Gtk.Drag.Finish (ctx, true, true, time);
			return w;
		}

		static void DragEnded (object obj, Gtk.DragEndArgs args)
		{
			dragWidget = null;
			if (DragEnd != null)
				DragEnd ();
		}

		public delegate void DNDDelegate ();

		public static event DNDDelegate DragBegin;
		public static event DNDDelegate DragEnd;
	}
}
