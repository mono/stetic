using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class VButtonBox : Gtk.VButtonBox, Stetic.IObjectWrapper, Stetic.IDesignTimeContainer {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static VButtonBox () {
			groups = new PropertyGroup[] {
				ButtonBox.ButtonBoxProperties,
				Widget.CommonWidgetProperties
			};
		}

		public VButtonBox ()
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

		private void ChildOccupancyChanged (IDesignTimeContainer container)
		{
			if (OccupancyChanged != null)
				OccupancyChanged (this);
		}
	}
}
