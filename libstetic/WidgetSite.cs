using System;

namespace Stetic {

	public abstract class WidgetSite : WidgetBox, IWidgetSite {
		public abstract Gtk.Widget Contents { get; }
		public abstract IWidgetSite ParentSite { get; }

		public abstract bool Occupied { get; }
		public abstract event OccupancyChangedHandler OccupancyChanged;

		public abstract bool HExpandable { get; }
		public abstract bool VExpandable { get; }

		public abstract Gtk.Requisition EmptySize { get; set; }

		public abstract void Select ();
		public abstract void UnSelect ();

		public abstract void Delete ();
	}

	public delegate void OccupancyChangedHandler (WidgetSite site);
}
