using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("VButtonBox", "vbuttonbox.png", WidgetType.Container)]
	public class VButtonBox : Gtk.VButtonBox, Stetic.IContainerWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static VButtonBox () {
			groups = new PropertyGroup[] {
				ButtonBox.ButtonBoxProperties,
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[] {
				ButtonBox.ButtonBoxChildProperties
			};
		}

		IStetic stetic;

		public VButtonBox (IStetic stetic)
		{
			this.stetic = stetic;
			for (int i = 0; i < 3; i++) {
				WidgetSite site = stetic.CreateWidgetSite ();
				site.OccupancyChanged += SiteOccupancyChanged;
				PackStart (site);
			}
		}

		public bool HExpandable { get { return false; } }
		public bool VExpandable { get { return true; } }

		public event ExpandabilityChangedHandler ExpandabilityChanged;

		private void SiteOccupancyChanged (WidgetSite site)
		{
			if (ExpandabilityChanged != null)
				ExpandabilityChanged (this);
		}
	}
}
