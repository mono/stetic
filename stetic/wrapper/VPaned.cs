using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class VPaned : Gtk.VPaned, IWidgetSite {

		public VPaned ()
		{
			WidgetSite site;

			site = new WidgetSite ();
			site.OccupancyChanged += ChildOccupancyChanged;
			Pack1 (site, true, false);

			site = new WidgetSite ();
			site.OccupancyChanged += ChildOccupancyChanged;
			Pack2 (site, true, false);
		}

		public bool HExpandable {
			get {
				foreach (Widget w in Children) {
					WidgetSite site = (WidgetSite)w;

					if (!site.HExpandable)
						return false;
				}
				return true;
			}
		}
		public bool VExpandable { get { return true; } }

		public event OccupancyChangedHandler OccupancyChanged;

		private void ChildOccupancyChanged (IWidgetSite site)
		{
			if (OccupancyChanged != null)
				OccupancyChanged (this);
		}
	}
}
