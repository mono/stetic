using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("HPaned", "hpaned.png")]
	public class HPaned : Gtk.HPaned, Stetic.IContainerWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static HPaned () {
			groups = new PropertyGroup[] {
				Paned.PanedProperties,
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[] {
				Paned.PanedChildProperties
			};
		}

		public HPaned (IStetic stetic)
		{
			WidgetSite site;

			site = stetic.CreateWidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			Pack1 (site, true, false);

			site = stetic.CreateWidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			Pack2 (site, true, false);
		}

		public bool HExpandable { get { return true; } }
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

		public event ExpandabilityChangedHandler ExpandabilityChanged;

		private void SiteOccupancyChanged (WidgetSite site)
		{
			if (ExpandabilityChanged != null)
				ExpandabilityChanged (this);
		}
	}
}
