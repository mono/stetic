using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;

namespace Stetic {

	public class HButtonBoxWrapper : Gtk.HButtonBox, IWidgetSite {

		public HButtonBoxWrapper ()
		{
			for (int i = 0; i < 2; i++) {
				WidgetSite site = new WidgetSite ();
				site.OccupancyChanged += ChildOccupancyChanged;
				PackStart (site);
			}
		}

		public bool HExpandable { get { return true; } }
		public bool VExpandable { get { return false; } }

		public event OccupancyChangedHandler OccupancyChanged;

		private void ChildOccupancyChanged (IWidgetSite site)
		{
			if (OccupancyChanged != null)
				OccupancyChanged (this);
		}
	}
}
