using Gtk;
using Gdk;
using System;
using System.Collections;

namespace Stetic {

	public class WidgetSite : WidgetBox {

		public WidgetSite (Widget child)
		{
			Add (child);
		}

		public override bool Internal {
			get {
				return base.Internal;
			}
			set {
				if (!base.Internal && value)
					DND.SourceUnset (this);
				else if (base.Internal && !value)
					DND.SourceSet (this, false);
				base.Internal = value;
			}
		}

		public override bool HExpandable {
			get {
				Stetic.Wrapper.Widget child = Stetic.Wrapper.Widget.Lookup (Child);
				return child.HExpandable;
			}
		}

		public override bool VExpandable {
			get {
				Stetic.Wrapper.Widget child = Stetic.Wrapper.Widget.Lookup (Child);
				return child.VExpandable;
			}
		}

		Hashtable faults;
		Set hfaults;
		const int FaultOverlap = 3;

		public void AddFault (object id, Gtk.Orientation orientation, Rectangle fault)
		{
			AddFault (id, orientation, fault.X, fault.Y, fault.Width, fault.Height);
		}

		public void AddFault (object id, Gtk.Orientation orientation,
				      int x, int y, int width, int height)
		{
			if (!IsRealized)
				return;

			Gdk.Window win = NewWindow (GdkWindow, Gdk.WindowClass.InputOnly);
			win.MoveResize (x, y, width, height);

			if (faults == null || faults.Count == 0) {
				if (faults == null) {
					faults = new Hashtable ();
					hfaults = new Set ();
					DND.DragBegin += ShowFaults;
					DND.DragEnd += HideFaults;
				}
				DND.DestSet (this, false);
			}

			faults[id] = win;
			hfaults[id] = (orientation == Gtk.Orientation.Horizontal);
		}

		public void AddHFault (object id, WidgetSite above, WidgetSite below)
		{
			if (!IsRealized)
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
				y1 = Math.Min (aboveAlloc.Y + aboveAlloc.Height, Allocation.Height - FaultOverlap);
				y2 = Allocation.Height;
			}

			AddFault (id, Gtk.Orientation.Horizontal, x1, y1, x2 - x1, y2 - y1);
		}

		public void AddVFault (object id, WidgetSite left, WidgetSite right)
		{
			if (!IsRealized)
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

				x1 = Math.Min (leftAlloc.X + leftAlloc.Width, Allocation.Width - FaultOverlap);
				x2 = Allocation.Width;

				y1 = leftAlloc.Y;
				y2 = leftAlloc.Y + leftAlloc.Height;
			}

			AddFault (id, Gtk.Orientation.Vertical, x1, y1, x2 - x1, y2 - y1);
		}

		public void ClearFaults ()
		{
			if (faults != null) {
				foreach (Gdk.Window win in faults.Values)
					win.Destroy ();
				faults.Clear ();
				hfaults.Clear ();
			}
			DND.DestUnset (this);
		}

		void ShowFaults ()
		{
			foreach (Gdk.Window win in faults.Values)
				win.Show ();
		}

		void HideFaults ()
		{
			foreach (Gdk.Window win in faults.Values)
				win.Hide ();
			OnDragLeave (null, 0);
		}

		object dragFault;
		Gdk.Window splitter;

		void FindFault (int x, int y, out object fault, out Gdk.Window win)
		{
			int wx, wy, width, height, depth;

			fault = null;
			win = null;
			wx = wy = width = height = 0;

			foreach (object id in faults.Keys) {
				win = faults[id] as Gdk.Window;
				win.GetGeometry (out wx, out wy, out width, out height, out depth);
				if (x >= wx && y >= wy && x <= wx + width && y <= wy + height) {
					fault = id;
					return;
				}
			}
		}

		// This is only called when dragging in fault mode, not in
		// placeholder mode.
		protected override bool OnDragMotion (Gdk.DragContext ctx, int x, int y, uint time)
		{
			if (faults == null || faults.Count == 0)
				return false;

			int wx, wy, width, height, depth;
			object match;
			Gdk.Window matchWin;
			
			FindFault (x, y, out match, out matchWin);

			// If there's a splitter visible, and we're not currently dragging
			// in the fault that owns that splitter, hide it
			if (splitter != null && dragFault != match) {
				splitter.Hide ();
				splitter.Destroy ();
				splitter = null;
			}

			if (dragFault != match) {
				dragFault = match;
				if (dragFault == null)
					return false;

				splitter = NewWindow (GdkWindow, Gdk.WindowClass.InputOutput);
				matchWin.GetGeometry (out wx, out wy, out width, out height, out depth);
				if (hfaults[dragFault]) {
					splitter.MoveResize (wx, wy + height / 2 - FaultOverlap,
							     width, 2 * FaultOverlap);
				} else {
					splitter.MoveResize (wx + width / 2 - FaultOverlap, wy,
							     2 * FaultOverlap, height);
				}
				splitter.ShowUnraised ();
				GdkWindow.Lower ();
			} else if (dragFault == null)
				return false;

			Gdk.Drag.Status (ctx, Gdk.DragAction.Move, time);
			return true;
		}

		protected override void OnDragLeave (Gdk.DragContext ctx, uint time)
		{
			if (splitter != null) {
				splitter.Hide ();
				splitter.Destroy ();
				splitter = null;
				dragFault = null;
			}
		}

		public delegate void DropOnHandler (Widget w, object faultId);
		public event DropOnHandler DropOn;

		protected virtual void Drop (Widget w, int x, int y)
		{
			if (faults != null && DropOn != null) {
				object faultId = null;
				Gdk.Window win;
				FindFault (x, y, out faultId, out win);
				DropOn (w, faultId);
			} else
				Add (w);

			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (w);
			if (wrapper != null)
				wrapper.Select ();
		}

		protected override bool OnDragDrop (DragContext ctx, int x, int y, uint time)
		{
			Widget dragged = DND.Drop (ctx, time);
			if (dragged == null)
				return false;

			Drop (dragged, x, y);
			return true;
		}

		public override string ToString ()
		{
			if (Child.Name == null)
				return "[WidgetSite " + GetHashCode().ToString() + ": " + Child.ToString() + " " + Child.GetHashCode().ToString() + "]";
			else
				return "[WidgetSite " + GetHashCode().ToString() + ": " + Child.ToString() + " '" + Child.Name + "' " + Child.GetHashCode().ToString() + "]";
		}
	}
}
