using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;

namespace Stetic {

	public class VButtonBoxWrapper : Gtk.VButtonBox, IWidgetSite {

		public VButtonBoxWrapper ()
		{
			for (int i = 0; i < 3; i++) {
				WidgetSite site = new WidgetSite ();
				site.OccupancyChanged += ChildOccupancyChanged;
				PackStart (site);
			}
		}

		public bool HExpandable { get { return false; } }
		public bool VExpandable { get { return true; } }

		public event OccupancyChangedHandler OccupancyChanged;

		private void ChildOccupancyChanged (IWidgetSite site)
		{
			if (OccupancyChanged != null)
				OccupancyChanged (this);
		}
	}
}
