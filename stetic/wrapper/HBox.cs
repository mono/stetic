using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class HBox : Gtk.HBox, Stetic.IObjectWrapper, Stetic.IDesignTimeContainer {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static HBox () {
			groups = new PropertyGroup[] {
				Box.BoxProperties,
				Widget.CommonWidgetProperties
			};
		}

		public HBox (bool homogeneous, int spacing) : base (homogeneous, spacing)
		{
			for (int i = 0; i < 3; i++) {
				WidgetSite site = new WidgetSite ();
				site.OccupancyChanged += ChildOccupancyChanged;
				PackStart (site);
			}
		}

		public bool HExpandable {
			get {
				foreach (Gtk.Widget w in Children) {
					WidgetSite site = (WidgetSite)w;

					if (site.HExpandable)
						return true;
				}
				return false;
			}
		}

		public bool VExpandable {
			get {
				foreach (Gtk.Widget w in Children) {
					WidgetSite site = (WidgetSite)w;

					if (!site.VExpandable)
						return false;
				}
				return true;
			}
		}

		public event OccupancyChangedHandler OccupancyChanged;

		private void ChildOccupancyChanged (IDesignTimeContainer container)
		{
			if (OccupancyChanged != null)
				OccupancyChanged (this);
		}
	}
}
