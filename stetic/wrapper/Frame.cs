using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class Frame : Gtk.Frame, IWidgetSite {

		public Frame (string label) : base (label)
		{
			WidgetSite site = new WidgetSite ();
			site.OccupancyChanged += ChildOccupancyChanged;
			Add (site);
		}

		public bool HExpandable { get { return ((WidgetSite)Child).HExpandable; } }
		public bool VExpandable { get { return ((WidgetSite)Child).VExpandable; } }

		public event OccupancyChangedHandler OccupancyChanged;

		private void ChildOccupancyChanged (IWidgetSite site)
		{
			if (OccupancyChanged != null)
				OccupancyChanged (this);
		}
	}
}
