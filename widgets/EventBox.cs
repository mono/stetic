using Gtk;
using System;
using System.Collections;

namespace Stetic.Widget {

	[WidgetWrapper ("EventBox", "eventbox.png")]
	public class EventBox : Gtk.EventBox, Stetic.IContainerWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup EventBoxProperties;

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static EventBox () {
			EventBoxProperties = new PropertyGroup ("EventBox Properties",
								typeof (Stetic.Widget.EventBox),
								"AboveChild",
								"VisibleWindow",
								"BorderWidth");
			groups = new PropertyGroup[] {
				EventBoxProperties,
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[0];
		}

		WidgetSite site;

		public EventBox (IStetic stetic)
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
