using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;

namespace Stetic {

	public class WidgetSiteImpl : WidgetSite {

		static TargetEntry[] Targets;
		static TargetList TargetList;
		static Gdk.Atom SteticWidgetType;

		static WidgetSiteImpl ()
		{
			SteticWidgetType = Gdk.Atom.Intern ("application/x-stetic-widget", false);

			Targets = new TargetEntry[1];
			Targets[0] = new TargetEntry ("application/x-stetic-widget", 0, 0);

			TargetList = new TargetList ();
			TargetList.Add (SteticWidgetType, 0, 0);
		}

		public WidgetSiteImpl () : this (10, 10) {}

		public WidgetSiteImpl (int emptyWidth, int emptyHeight)
		{
			WidgetFlags |= WidgetFlags.CanFocus;

			emptySize.Width = emptyWidth;
			emptySize.Height = emptyHeight;
			Occupancy = SiteOccupancy.Empty;
		}

		public override Widget Contents {
			get {
				return Child;
			}
		}

		public override IWidgetSite ParentSite {
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

		public override event OccupancyChangedHandler OccupancyChanged;

		public override bool Occupied {
			get { return (Occupancy != SiteOccupancy.Empty); }
		}

		public override bool HExpandable {
			get {
				if (Occupancy == SiteOccupancy.Empty)
					return true;

				Stetic.Wrapper.Container child;
				if (Occupancy == SiteOccupancy.PseudoOccupied)
					child = Stetic.Wrapper.Container.Lookup (dragWidget);
				else
					child = Stetic.Wrapper.Container.Lookup (Child);

				if (child != null)
					return child.HExpandable;
				else
					return false;
			}
		}

		public override bool VExpandable {
			get {
				if (Occupancy == SiteOccupancy.Empty)
					return true;

				Stetic.Wrapper.Container child;
				if (Occupancy == SiteOccupancy.PseudoOccupied)
					child = Stetic.Wrapper.Container.Lookup (dragWidget);
				else
					child = Stetic.Wrapper.Container.Lookup (Child);

				if (child != null)
					return child.VExpandable;
				else
					return false;
			}
		}

		protected override bool OnChildButtonPress (Gdk.EventButton evt)
		{
			if (evt.Button == 3 && evt.Type == EventType.ButtonPress)
				return OnPopupMenu ();

			if (!ShowHandles) {
				GrabFocus ();
				return true;
			} else
				return false;
		}

		protected int clickX, clickY;
		protected override bool OnButtonPressEvent (Gdk.EventButton evt)
		{
			if (base.OnButtonPressEvent (evt))
				return true;

			if (evt.Button == 3 && evt.Type == EventType.ButtonPress)
				return OnPopupMenu ();

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
			WidgetSiteImpl source;
			Widget dragged;
			Container parent;

			source = Gtk.Drag.GetSourceWidget (ctx) as WidgetSiteImpl;
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

		protected override bool OnPopupMenu ()
		{
			Menu m = new ContextMenu (this);
			m.Popup ();
			return true;
		}

		public void Focus ()
		{
			ShowHandles = true;
		}

		public void UnFocus ()
		{
			ShowHandles = false;
		}

		public override void Select ()
		{
			GrabFocus ();
		}

		public override void UnSelect ()
		{
			UnFocus ();
		}

		public override void Delete ()
		{
			if (Child != null) {
				Remove (Child);
				if (OccupancyChanged != null)
					OccupancyChanged (this);
			}
		}

	}

	public class WindowSite : IWidgetSite {
		Gtk.Window contents;

		static Hashtable windowSites = new Hashtable ();

		public WindowSite (Gtk.Window contents)
		{
			this.contents = contents;
			windowSites[contents] = this;
			contents.DeleteEvent += ContentsDeleted;
			contents.SetFocus += ContentsSetFocus;
		}

		void ContentsDeleted (object obj, EventArgs args)
		{
			if (FocusChanged != null) {
				FocusChanged (this, null);
				FocusChanged = null;
			}
			windowSites.Remove (contents);
		}

		public delegate void FocusChangedHandler (WindowSite site, IWidgetSite focus);
		public event FocusChangedHandler FocusChanged;

		[ConnectBefore] // otherwise contents.Focus will be the new focus, not the old
		void ContentsSetFocus (object obj, SetFocusArgs args)
		{
			Widget w;

			w = contents.Focus;
			while (w != null && !(w is WidgetSiteImpl))
				w = w.Parent;
			WidgetSiteImpl oldf = (WidgetSiteImpl)w;

			w = args.Focus;
			while (w != null && !(w is WidgetSiteImpl))
				w = w.Parent;
			WidgetSiteImpl newf = (WidgetSiteImpl)w;

			if (oldf == newf)
				return;

			if (oldf != null)
				oldf.UnFocus ();
			if (newf != null)
				newf.Focus ();

			if (FocusChanged != null)
				FocusChanged (this, newf != null ? (IWidgetSite)newf : (IWidgetSite)this);
		}

		public static IWidgetSite LookupSite (Widget window)
		{
			return windowSites[window] as IWidgetSite;
		}

		public Widget Contents {
			get {
				return contents;
			}
		}

		public IWidgetSite ParentSite {
			get {
				return null;
			}
		}

		public bool Occupied {
			get {
				return true;
			}
		}

		public void Delete ()
		{
			contents.Destroy ();
		}

		public void Select ()
		{
			contents.Focus = null;
		}

		public void UnSelect ()
		{
			;
		}
	}
}
