using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;

namespace Stetic {

	public class FrameWrapper : Gtk.Frame, IWidgetSite {

		public FrameWrapper (string label) : base (label)
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
