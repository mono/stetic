using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class HButtonBox : Gtk.HButtonBox, Stetic.IObjectWrapper, Stetic.IDesignTimeContainer {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static HButtonBox () {
			groups = new PropertyGroup[] {
				ButtonBox.ButtonBoxProperties,
				Widget.CommonWidgetProperties
			};
		}

		public HButtonBox ()
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

		private void ChildOccupancyChanged (IDesignTimeContainer container)
		{
			if (OccupancyChanged != null)
				OccupancyChanged (this);
		}
	}
}
