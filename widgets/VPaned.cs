using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("VPaned", "vpaned.png")]
	public class VPaned : Gtk.VPaned, Stetic.IContainerWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static VPaned () {
			groups = new PropertyGroup[] {
				Paned.PanedProperties,
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[] {
				Paned.PanedChildProperties
			};
		}

		public VPaned (IStetic stetic)
		{
			WidgetSite site;

			site = stetic.CreateWidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			Pack1 (site, true, false);

			site = stetic.CreateWidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			Pack2 (site, true, false);
		}

		public bool HExpandable {
			get {
				foreach (Gtk.Widget w in Children) {
					WidgetSite site = (WidgetSite)w;

					if (!site.HExpandable)
						return false;
				}
				return true;
			}
		}
		public bool VExpandable { get { return true; } }

		public event ExpandabilityChangedHandler ExpandabilityChanged;

		private void SiteOccupancyChanged (WidgetSite site)
		{
			if (ExpandabilityChanged != null)
				ExpandabilityChanged (this);
		}
	}
}
