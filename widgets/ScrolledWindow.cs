using Gtk;
using System;
using System.Collections;

namespace Stetic.Widget {

	[WidgetWrapper ("ScrolledWindow", "scrolledwindow.png")]
	public class ScrolledWindow : Gtk.ScrolledWindow, Stetic.IContainerWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup ScrolledWindowProperties;

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static ScrolledWindow () {
			ScrolledWindowProperties = new PropertyGroup ("ScrolledWindow Properties",
								      typeof (Stetic.Widget.ScrolledWindow),
								      "VscrollbarPolicy",
								      "HscrollbarPolicy",
								      "ShadowType",
								      "WindowPlacement",
								      "BorderWidth");
			groups = new PropertyGroup[] {
				ScrolledWindowProperties,
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[0];
		}

		public ScrolledWindow (IStetic stetic)
		{
			SetPolicy (PolicyType.Always, PolicyType.Always);
			WidgetSite site = stetic.CreateWidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			AddWithViewport (site);
		}

		public bool HExpandable { get { return true; } }
		public bool VExpandable { get { return true; } }

		public event ContentsChangedHandler ContentsChanged;

		void SiteOccupancyChanged (WidgetSite site)
		{
			if (Child == null)
				return;

			if (site.Occupied &&
			    site.Contents.SetScrollAdjustments (null, null)) {
				if (Child is Gtk.Viewport) {
					((Gtk.Viewport)Child).Remove (site);
					Remove (Child);
					Add (site);
				}
			} else {
				if (!(Child is Gtk.Viewport)) {
					Remove (Child);
					AddWithViewport (site);
				}
			}

			if (ContentsChanged != null)
				ContentsChanged (this);
		}
	}
}
