using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("HButtonBox", "hbuttonbox.png", WidgetType.Container)]
	public class HButtonBox : Gtk.HButtonBox, Stetic.IContainerWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static HButtonBox () {
			groups = new PropertyGroup[] {
				ButtonBox.ButtonBoxProperties,
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[] {
				ButtonBox.ButtonBoxChildProperties
			};
		}

		IStetic stetic;

		public HButtonBox (IStetic stetic)
		{
			this.stetic = stetic;
			for (int i = 0; i < 2; i++) {
				WidgetSite site = stetic.CreateWidgetSite ();
				site.OccupancyChanged += SiteOccupancyChanged;
				PackStart (site);
			}
		}

		public bool HExpandable { get { return true; } }
		public bool VExpandable { get { return false; } }

		public event ExpandabilityChangedHandler ExpandabilityChanged;

		private void SiteOccupancyChanged (WidgetSite site)
		{
			if (ExpandabilityChanged != null)
				ExpandabilityChanged (this);
		}
	}
}
