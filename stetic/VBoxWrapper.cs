using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;

namespace Stetic {

	public class VBoxWrapper : Gtk.VBox, IWidgetSite {

		public VBoxWrapper (bool homogeneous, int spacing) : base (homogeneous, spacing)
		{
			for (int i = 0; i < 3; i++) {
				WidgetSite site = new WidgetSite ();
				site.OccupancyChanged += ChildOccupancyChanged;
				PackStart (site);
			}
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

		public bool VExpandable {
			get {
				foreach (Widget w in Children) {
					WidgetSite site = (WidgetSite)w;

					if (site.VExpandable)
						return true;
				}
				return false;
			}
		}

		public event OccupancyChangedHandler OccupancyChanged;

		private void ChildOccupancyChanged (IWidgetSite site)
		{
			if (OccupancyChanged != null)
				OccupancyChanged (this);
		}
	}
}
