using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic {

	public delegate void OccupancyChangedHandler (IWidgetSite holder);

	public interface IWidgetSite {
		bool HExpandable { get; }
		bool VExpandable { get; }

		event OccupancyChangedHandler OccupancyChanged;
	}

	public class WidgetSite : WidgetBox, IWidgetSite {

		static TargetEntry[] Targets;
		static TargetList TargetList;
		static Gdk.Atom SteticWidgetType;

		static WidgetSite ()
		{
			SteticWidgetType = Gdk.Atom.Intern ("application/x-stetic-widget", false);

			Targets = new TargetEntry[1];
			Targets[0] = new TargetEntry ("application/x-stetic-widget", 0, 0);

			TargetList = new TargetList ();
			TargetList.Add (SteticWidgetType, 0, 0);
		}

		public WidgetSite ()
		{
			Flags |= (int)WidgetFlags.CanFocus;
			Occupancy = SiteOccupancy.Empty;
		}

		public WidgetSite (Widget w)
		{
			Add (w);
		}

		private void ChildOccupancyChanged (IWidgetSite holder)
		{
			if (OccupancyChanged != null)
				OccupancyChanged (this);
		}

		protected override void OnAdded (Widget child)
		{
			base.OnAdded (child);
			ShowPlaceholder = false;
			if (child is IWidgetSite)
				((IWidgetSite)child).OccupancyChanged += ChildOccupancyChanged;
			else
				InterceptEvents = true;
			Occupancy = SiteOccupancy.Occupied;
		}

		protected override void OnRemoved (Widget w)
		{
			if (Occupancy == SiteOccupancy.Occupied)
				Occupancy = SiteOccupancy.Empty;
			if (w is IWidgetSite)
				((IWidgetSite)w).OccupancyChanged -= ChildOccupancyChanged;
			ShowPlaceholder = true;
			InterceptEvents = false;
			base.OnRemoved (w);
		}

		public enum SiteOccupancy { Empty, Occupied, PseudoOccupied };

		private SiteOccupancy state;
		private SiteOccupancy Occupancy {
			get { return state; }
			set {
				state = value;
				switch (state) {
				case SiteOccupancy.Empty:
					SetSizeRequest (10, 10);
					Gtk.Drag.DestSet (this, DestDefaults.All,
							  Targets, DragAction.Move);
					if (OccupancyChanged != null)
						OccupancyChanged (this);
					break;

				case SiteOccupancy.Occupied:
					SetSizeRequest (-1, -1);
					Gtk.Drag.DestUnset (this);
					if (OccupancyChanged != null)
						OccupancyChanged (this);
					break;

				case SiteOccupancy.PseudoOccupied:
					SetSizeRequest (Child.ChildRequisition.Width,
							Child.ChildRequisition.Height);
					Gtk.Drag.DestSet (this, DestDefaults.All,
							  Targets, DragAction.Move);
					break;
				}
			}
		}

		public event OccupancyChangedHandler OccupancyChanged;

		public bool Occupied {
			get { return (Occupancy != SiteOccupancy.Empty); }
		}

		public bool HExpandable {
			get {
				if (Occupancy == SiteOccupancy.Empty)
					return true;

				Widget child;
				if (Occupancy == SiteOccupancy.PseudoOccupied)
					child = dragWidget;
				else
					child = Child;
				if (child == null)
					return true;

				if (child is IWidgetSite)
					return ((IWidgetSite)child).HExpandable;
				else
					return false;
			}
		}

		public bool VExpandable {
			get {
				if (Occupancy == SiteOccupancy.Empty)
					return true;

				Widget child;
				if (Occupancy == SiteOccupancy.PseudoOccupied)
					child = dragWidget;
				else
					child = Child;
				if (child == null)
					return true;

				if (child is IWidgetSite)
					return ((IWidgetSite)child).VExpandable;
				else
					return false;
			}
		}

		protected int clickX, clickY;
		protected override bool OnButtonPressEvent (Gdk.EventButton evt)
		{
			if (base.OnButtonPressEvent (evt))
				return true;

			clickX = (int)evt.XRoot;
			clickY = (int)evt.YRoot;
			GrabFocus ();
			return true;
		}

		protected override bool OnButtonReleaseEvent (Gdk.EventButton evt)
		{
			if (ShowHandles)
				InterceptEvents = false;
			return false;
		}

		protected Widget dragWidget;

		protected virtual bool StartDrag ()
		{
			dragWidget = Child;
			if (dragWidget == null)
				return false;

			Occupancy = SiteOccupancy.PseudoOccupied;
			Remove (dragWidget);
			return true;
		}

		protected override bool OnMotionNotifyEvent (Gdk.EventMotion evt)
		{
			Gtk.Window dragWin;
			DragContext ctx;
			int mx, my, wx, wy;
			Requisition req;

			if ((evt.State & ModifierType.Button1Mask) == 0)
				return true;
			if (!Gtk.Drag.CheckThreshold (this, clickX, clickY, (int)evt.XRoot, (int)evt.YRoot))
				return true;

			if (!StartDrag ())
				return true;

			mx = (int)evt.XRoot;
			my = (int)evt.YRoot;

			GdkWindow.GetOrigin (out wx, out wy);

			dragWin = new Gtk.Window (Gtk.WindowType.Popup);
			dragWin.Add (dragWidget);
			req = dragWidget.SizeRequest ();

			if (wx + req.Width < mx)
				wx = mx - req.Width / 2;
			if (wy + req.Height < my)
				wy = my - req.Height / 2;

			dragWin.Move (wx, wy);
			dragWin.Show ();

			ctx = Gtk.Drag.Begin (this, TargetList,
					      DragAction.Move,
					      1, evt);
			Gtk.Drag.SetIconWidget (ctx, dragWin, mx - wx, my - wy);

			return false;
		}

		protected override bool OnDragDrop (DragContext ctx,
						    int x, int y, uint time)
		{
			WidgetSite source;
			Widget dragged;
			Container parent;

			source = Gtk.Drag.GetSourceWidget (ctx) as WidgetSite;
			if (source == null) {
				Gtk.Drag.Finish (ctx, false, false, time);
				return false;
			}

			dragged = source.dragWidget;
			source.dragWidget = null;
			if (dragged == null) {
				Gtk.Drag.Finish (ctx, false, false, time);
				return false;
			}

			parent = dragged.Parent as Container;
			if (parent != null)
				parent.Remove (dragged);

			Add (dragged);
			GrabFocus ();
			Gtk.Drag.Finish (ctx, true, false, time);
			return true;
		}

		protected override void OnDragEnd (DragContext ctx)
		{
			if (dragWidget != null) {
				Container parent;

				parent = dragWidget.Parent as Container;
				if (parent != null)
					parent.Remove (dragWidget);
				Add (dragWidget);
			} else if (Child == null)
				Occupancy = SiteOccupancy.Empty;

			dragWidget = null;
		}

		protected override bool OnKeyReleaseEvent (Gdk.EventKey evt)
		{
			if (Child != null && evt.Key == Gdk.Key.Delete) {
				Remove (Child);
				if (OccupancyChanged != null)
					OccupancyChanged (this);
			}
			return false;
		}

		public void Focus ()
		{
			ShowHandles = true;
		}

		public void UnFocus ()
		{
			ShowHandles = false;
			if (Child != null && !(Child is IWidgetSite))
				InterceptEvents = true;
		}
	}
}
