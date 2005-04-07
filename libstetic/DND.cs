using System;
using System.Collections;

namespace Stetic {

	public static class DND {
		static Gtk.TargetEntry[] targets;
		static Gtk.TargetList targetList;
		static Gdk.Atom steticWidgetType;

		static DND ()
		{
			steticWidgetType = Gdk.Atom.Intern ("application/x-stetic-widget", false);

			targets = new Gtk.TargetEntry[1];
			targets[0] = new Gtk.TargetEntry ("application/x-stetic-widget", 0, 0);

			targetList = new Gtk.TargetList ();
			targetList.Add (steticWidgetType, 0, 0);
		}

		public static void SourceSet (Gtk.Widget source)
		{
			Gtk.Drag.SourceSet (source, Gdk.ModifierType.Button1Mask,
					    targets, Gdk.DragAction.Move);
		}

		public static void SourceUnset (Gtk.Widget source)
		{
			Gtk.Drag.SourceUnset (source);
		}

		public static void DestSet (Gtk.Widget dest, bool automatic)
		{
			Gtk.Drag.DestSet (dest, automatic ? Gtk.DestDefaults.All : 0,
					  targets, Gdk.DragAction.Move);
		}

		public static void DestUnset (Gtk.Widget dest)
		{
			Gtk.Drag.DestUnset (dest);
		}

		static Gtk.Widget dragWidget;

		// Drag function for non-automatic sources, called from MotionNotifyEvent
		public static void Drag (Gtk.Widget source, Gdk.EventMotion evt, Gtk.Widget dragWidget)
		{
			Gdk.DragContext ctx;

			ctx = Gtk.Drag.Begin (source, targetList, Gdk.DragAction.Move,
					      1 /* button */, evt);
			Drag (source, ctx, dragWidget);
		}

		// Drag function for automatic sources, called from DragBegin
		public static void Drag (Gtk.Widget source, Gdk.DragContext ctx, Gtk.Widget dragWidget)
		{
			Gtk.Window dragWin;
			Gtk.Requisition req;

			ShowFaults ();
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

			if (source != null)
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

			Gtk.Widget w = Cancel ();
			Gtk.Drag.Finish (ctx, true, true, time);
			return w;
		}

		// Call this from a DragEnd event to check if the widget wasn't dropped
		public static Gtk.Widget Cancel ()
		{
			Gtk.Widget w = dragWidget;
			dragWidget = null;

			// Remove the widget from its dragWindow
			Gtk.Container parent = w.Parent as Gtk.Container;
			if (parent != null) {
				parent.Remove (w);
				parent.Destroy ();
			}
			return w;
		}

		static void DragEnded (object obj, Gtk.DragEndArgs args)
		{
			dragWidget = null;
			HideFaults ();

			((Gtk.Widget)obj).DragEnd -= DragEnded;
		}

		class Fault {
			public Stetic.Wrapper.Widget Owner;
			public object Id;
			public Gtk.Orientation Orientation;
			public Gdk.Window Window;

			public Fault (Stetic.Wrapper.Widget owner, object id,
				      Gtk.Orientation orientation, Gdk.Window window)
			{
				Owner = owner;
				Id = id;
				Orientation = orientation;
				Window = window;
			}
		}

		static Hashtable faultGroups = new Hashtable ();
		const int FaultOverlap = 3;

		public static void AddFault (Stetic.Wrapper.Widget owner, object faultId,
					     Gtk.Orientation orientation, Gdk.Rectangle fault)
		{
			AddFault (owner, faultId, orientation,
				  fault.X, fault.Y, fault.Width, fault.Height);
		}

		public static void AddFault (Stetic.Wrapper.Widget owner, object faultId,
					     Gtk.Orientation orientation,
					     int x, int y, int width, int height)
		{
			Gtk.Widget widget = owner.Wrapped;
			if (!widget.IsRealized)
				return;

			Gdk.Window win = NewWindow (widget, Gdk.WindowClass.InputOnly);
			win.MoveResize (x, y, width, height);

			Hashtable widgetFaults = faultGroups[widget] as Hashtable;
			if (widgetFaults == null) {
				faultGroups[widget] = widgetFaults = new Hashtable ();
				widget.Destroyed += FaultWidgetDestroyed;
				widget.DragMotion += FaultDragMotion;
				widget.DragLeave += FaultDragLeave;
				widget.DragDrop += FaultDragDrop;
				DND.DestSet (widget, false);
			}
			widgetFaults[win] = new Fault (owner, faultId, orientation, win);
		}

		public static void AddHFault (Stetic.Wrapper.Widget owner, object faultId,
					      Gtk.Widget above, Gtk.Widget below)
		{
			Gtk.Widget widget = owner.Wrapped;
			if (!widget.IsRealized)
				return;

			Gdk.Rectangle aboveAlloc, belowAlloc;
			int x1, y1, x2, y2;

			if (above != null && below != null) {
				aboveAlloc = above.Allocation;
				belowAlloc = below.Allocation;

				x1 = Math.Min (aboveAlloc.X, belowAlloc.X);
				x2 = Math.Max (aboveAlloc.X + aboveAlloc.Width, belowAlloc.X + belowAlloc.Width);
				y1 = aboveAlloc.Y + aboveAlloc.Height;
				y2 = belowAlloc.Y;

				while (y2 - y1 < FaultOverlap * 2) {
					y1--;
					y2++;
				}
			} else if (above == null) {
				belowAlloc = below.Allocation;

				x1 = belowAlloc.X;
				x2 = belowAlloc.X + belowAlloc.Width;
				y1 = 0;
				y2 = Math.Max (belowAlloc.Y, FaultOverlap);
			} else {
				aboveAlloc = above.Allocation;

				x1 = aboveAlloc.X;
				x2 = aboveAlloc.X + aboveAlloc.Width;
				y1 = Math.Min (aboveAlloc.Y + aboveAlloc.Height, widget.Allocation.Height - FaultOverlap);
				y2 = widget.Allocation.Height;
			}

			AddFault (owner, faultId, Gtk.Orientation.Horizontal,
				  x1, y1, x2 - x1, y2 - y1);
		}

		public static void AddVFault (Stetic.Wrapper.Widget owner, object faultId,
					      Gtk.Widget left, Gtk.Widget right)
		{
			Gtk.Widget widget = owner.Wrapped;
			if (!widget.IsRealized)
				return;

			Gdk.Rectangle leftAlloc, rightAlloc;
			int x1, y1, x2, y2;

			if (left != null && right != null) {
				leftAlloc = left.Allocation;
				rightAlloc = right.Allocation;

				x1 = leftAlloc.X + leftAlloc.Width;
				x2 = rightAlloc.X;

				y1 = Math.Min (leftAlloc.Y, rightAlloc.Y);
				y2 = Math.Max (leftAlloc.Y + leftAlloc.Height, rightAlloc.Y + rightAlloc.Height);

				while (x2 - x1 < FaultOverlap * 2) {
					x1--;
					x2++;
				}
			} else if (left == null) {
				rightAlloc = right.Allocation;

				x1 = 0;
				x2 = Math.Max (rightAlloc.X, FaultOverlap);

				y1 = rightAlloc.Y;
				y2 = rightAlloc.Y + rightAlloc.Height;
			} else {
				leftAlloc = left.Allocation;

				x1 = Math.Min (leftAlloc.X + leftAlloc.Width, widget.Allocation.Width - FaultOverlap);
				x2 = widget.Allocation.Width;

				y1 = leftAlloc.Y;
				y2 = leftAlloc.Y + leftAlloc.Height;
			}

			AddFault (owner, faultId, Gtk.Orientation.Vertical,
				  x1, y1, x2 - x1, y2 - y1);
		}

		static void FaultWidgetDestroyed (object widget, EventArgs args)
		{
			ClearFaults ((Gtk.Widget)widget);
		}

		public static void ClearFaults (Stetic.Wrapper.Widget owner)
		{
			ClearFaults (owner.Wrapped);
		}

		static void ClearFaults (Gtk.Widget widget)
		{
			Hashtable widgetFaults = faultGroups[widget] as Hashtable;
			if (widgetFaults == null)
				return;
			faultGroups.Remove (widget);

			foreach (Gdk.Window win in widgetFaults.Keys)
				win.Destroy ();
			widgetFaults.Clear ();
			DND.DestUnset (widget);
		}

		static void ShowFaults ()
		{
			foreach (Hashtable widgetFaults in faultGroups.Values) {
				foreach (Gdk.Window win in widgetFaults.Keys)
					win.Show ();
			}
		}

		static void HideFaults ()
		{
			foreach (Hashtable widgetFaults in faultGroups.Values) {
				foreach (Gdk.Window win in widgetFaults.Keys)
					win.Hide ();
			}
			DestroySplitter ();
			dragFault = null;
		}

		static Fault dragFault;
		static Gdk.Window splitter;

		static void DestroySplitter ()
		{
			if (splitter != null) {
				splitter.Hide ();
				splitter.Destroy ();
				splitter = null;
			}
		}

		static void FindFault (int x, int y, out Fault fault)
		{
			int wx, wy, width, height, depth;

			fault = null;

			foreach (Hashtable widgetFaults in faultGroups.Values) {
				foreach (Fault f in widgetFaults.Values) {
					f.Window.GetGeometry (out wx, out wy, out width, out height, out depth);
					if (x >= wx && y >= wy && x <= wx + width && y <= wy + height) {
						fault = f;
						return;
					}
				}
			}
		}

		static void FaultDragMotion (object obj, Gtk.DragMotionArgs args)
		{
			int wx, wy, width, height, depth;
			Fault fault;

			FindFault (args.X, args.Y, out fault);

			// If there's a splitter visible, and we're not currently dragging
			// in the fault that owns that splitter, hide it
			if (splitter != null && dragFault != fault)
				DestroySplitter ();

			if (dragFault != fault) {
				dragFault = fault;
				if (dragFault == null)
					return;

				splitter = NewWindow (fault.Owner.Wrapped, Gdk.WindowClass.InputOutput);
				fault.Window.GetGeometry (out wx, out wy, out width, out height, out depth);
				if (fault.Orientation == Gtk.Orientation.Horizontal) {
					splitter.MoveResize (wx, wy + height / 2 - FaultOverlap,
							     width, 2 * FaultOverlap);
				} else {
					splitter.MoveResize (wx + width / 2 - FaultOverlap, wy,
							     2 * FaultOverlap, height);
				}
				splitter.ShowUnraised ();
				fault.Window.Lower ();
			} else if (dragFault == null)
				return;

			Gdk.Drag.Status (args.Context, Gdk.DragAction.Move, args.Time);
			args.RetVal = true;
		}

		static void FaultDragLeave (object obj, Gtk.DragLeaveArgs args)
		{
			DestroySplitter ();
			dragFault = null;
		}

		static void FaultDragDrop (object obj, Gtk.DragDropArgs args)
		{
			Gtk.Widget dragged = DND.Drop (args.Context, args.Time);
			if (dragged == null)
				return;

			Fault fault;
			FindFault (args.X, args.Y, out fault);
			fault.Owner.Drop (dragged, fault.Id);

			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (dragged);
			if (wrapper != null)
				wrapper.Select ();

			args.RetVal = true;
		}

		static Gdk.Window NewWindow (Gtk.Widget parent, Gdk.WindowClass wclass)
		{
			Gdk.WindowAttr attributes;
			Gdk.WindowAttributesType attributesMask;
			Gdk.Window win;

			attributes = new Gdk.WindowAttr ();
			attributes.WindowType = Gdk.WindowType.Child;
			attributes.Wclass = wclass ;
			attributes.visual = parent.Visual;
			attributes.colormap = parent.Colormap;
			attributes.Mask = (Gdk.EventMask.ButtonPressMask |
					   Gdk.EventMask.ButtonMotionMask |
					   Gdk.EventMask.ButtonReleaseMask |
					   Gdk.EventMask.ExposureMask |
					   Gdk.EventMask.EnterNotifyMask |
					   Gdk.EventMask.LeaveNotifyMask);

			attributesMask =
				Gdk.WindowAttributesType.Visual |
				Gdk.WindowAttributesType.Colormap;

			win = new Gdk.Window (parent.GdkWindow, attributes, attributesMask);
			win.UserData = parent.Handle;

			if (wclass == Gdk.WindowClass.InputOutput)
				parent.Style.Attach (win);

			return win;
		}
	}
}
