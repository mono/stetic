using Gtk;
using Gdk;
using System;

namespace Stetic {

	public delegate void OccupancyChangedHandler (WidgetSite site);

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
			WidgetFlags |= WidgetFlags.CanFocus;

			emptySize.Width = emptySize.Height = 10;
			Occupancy = SiteOccupancy.Empty;
		}

		public Widget Contents {
			get {
				return Child;
			}
		}

		public IWidgetSite ParentSite {
			get {
				for (Widget w = Parent; ; w = w.Parent) {
					if (w == null)
						return null;
					if (w is IWidgetSite)
						return w as IWidgetSite;
					if (w.Parent == null)
						return WindowSite.LookupSite (w);
				}
			}
		}

		private void ChildContentsChanged (Stetic.Wrapper.Container container)
		{
			if (OccupancyChanged != null)
				OccupancyChanged (this);
		}

		protected override void OnAdded (Widget child)
		{
			base.OnAdded (child);
			Occupancy = SiteOccupancy.Occupied;

			Stetic.Wrapper.Container container = Stetic.Wrapper.Container.Lookup (child);
			if (container != null)
				container.ContentsChanged += ChildContentsChanged;
		}

		protected override void OnRemoved (Widget w)
		{
			Stetic.Wrapper.Container container = Stetic.Wrapper.Container.Lookup (w);
			if (container != null)
				container.ContentsChanged -= ChildContentsChanged;

			if (Occupancy == SiteOccupancy.Occupied)
				Occupancy = SiteOccupancy.Empty;
			base.OnRemoved (w);
		}

		Requisition emptySize;
		public Requisition EmptySize {
			get {
				return emptySize;
			}
			set {
				emptySize = value;
				if (Occupancy == SiteOccupancy.Empty)
					SetSizeRequest (emptySize.Width, emptySize.Height);
			}
		}

		public enum SiteOccupancy { Empty, Occupied, PseudoOccupied };

		private SiteOccupancy state;
		private SiteOccupancy Occupancy {
			get { return state; }
			set {
				state = value;
				switch (state) {
				case SiteOccupancy.Empty:
					SetSizeRequest (emptySize.Width, emptySize.Height);
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

				Stetic.Wrapper.Widget child;
				if (Occupancy == SiteOccupancy.PseudoOccupied)
					child = Stetic.Wrapper.Widget.Lookup (dragWidget);
				else
					child = Stetic.Wrapper.Widget.Lookup (Child);

				if (child != null)
					return child.HExpandable;
				else
					return false;
			}
		}

		public bool VExpandable {
			get {
				if (Occupancy == SiteOccupancy.Empty)
					return true;

				Stetic.Wrapper.Widget child;
				if (Occupancy == SiteOccupancy.PseudoOccupied)
					child = Stetic.Wrapper.Widget.Lookup (dragWidget);
				else
					child = Stetic.Wrapper.Widget.Lookup (Child);

				if (child != null)
					return child.VExpandable;
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

		protected Widget dragWidget;

		protected virtual bool StartDrag (Gdk.EventMotion evt)
		{
			if (evt.Window != HandleWindow)
				return false;

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
			int mx, my;
			Requisition req;

			if ((evt.State & ModifierType.Button1Mask) == 0)
				return true;
			if (!Gtk.Drag.CheckThreshold (this, clickX, clickY, (int)evt.XRoot, (int)evt.YRoot))
				return true;

			if (!StartDrag (evt))
				return true;

			mx = (int)evt.XRoot;
			my = (int)evt.YRoot;

			dragWin = new Gtk.Window (Gtk.WindowType.Popup);
			dragWin.Add (dragWidget);

			req = dragWidget.SizeRequest ();
			if (req.Width < 20 && req.Height < 20)
				dragWin.SetSizeRequest (20, 20);
			else if (req.Width < 20)
				dragWin.SetSizeRequest (20, -1);
			else if (req.Height < 20)
				dragWin.SetSizeRequest (-1, 20);

			dragWin.Move (mx, my);
			dragWin.Show ();

			ctx = Gtk.Drag.Begin (this, TargetList, DragAction.Move, 1, evt);
			Gtk.Drag.SetIconWidget (ctx, dragWin, 0, 0);

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
			if (evt.Key == Gdk.Key.Delete) {
				Delete ();
				return true;
			}
			return false;
		}

		public event EventHandler Selected;

		public void Focus ()
		{
			ShowHandles = true;
			if (Selected != null)
				Selected (this, EventArgs.Empty);
		}

		public void UnFocus ()
		{
			ShowHandles = false;
		}

		public void Select ()
		{
			GrabFocus ();
		}

		public void UnSelect ()
		{
			UnFocus ();
		}

		public void Delete ()
		{
			if (Child != null) {
				Remove (Child);
				if (OccupancyChanged != null)
					OccupancyChanged (this);
			}
		}

	}
}
