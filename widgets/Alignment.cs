using Gtk;
using System;
using System.Collections;

namespace Stetic.Widget {

	[WidgetWrapper ("Alignment", "alignment.png")]
	public class Alignment : Gtk.Alignment, Stetic.IContainerWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup AlignmentProperties;

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static Alignment () {
			AlignmentProperties = new PropertyGroup ("Alignment Properties",
								 typeof (Stetic.Widget.Alignment),
								 "Xscale",
								 "Yscale",
								 "Xalign",
								 "Yalign",
								 "LeftPadding",
								 "TopPadding",
								 "RightPadding",
								 "BottomPadding",
								 "BorderWidth");
			groups = new PropertyGroup[] {
				AlignmentProperties,
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[0];
		}

		WidgetSite site;

		public Alignment (IStetic stetic) : base (0.5f, 0.5f, 1.0f, 1.0f)
		{
			site = stetic.CreateWidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			Add (site);
		}

		public bool HExpandable { get { return site.HExpandable; } }
		public bool VExpandable { get { return site.VExpandable; } }

		public event ExpandabilityChangedHandler ExpandabilityChanged;

		private void SiteOccupancyChanged (WidgetSite site)
		{
			if (ExpandabilityChanged != null)
				ExpandabilityChanged (this);
		}
	}
}
