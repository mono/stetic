using Gtk;
using System;

namespace Stetic {

	public abstract class WidgetSite : WidgetBox, IWidgetSite {
		public abstract Widget Contents { get; }
		public abstract IWidgetSite ParentSite { get; }

		public abstract bool Occupied { get; }
		public abstract event OccupancyChangedHandler OccupancyChanged;

		public abstract bool HExpandable { get; }
		public abstract bool VExpandable { get; }

		public abstract void Select ();
		public abstract void UnSelect ();

		public abstract void Delete ();
	}

	public delegate void OccupancyChangedHandler (WidgetSite site);
}
