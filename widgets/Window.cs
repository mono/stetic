using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Window", "window.png", WidgetType.Window)]
	public class Window : Gtk.Window, Stetic.IContainerWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup WindowProperties;
		public static PropertyGroup WindowSizeProperties;
		public static PropertyGroup WindowMiscProperties;

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static Window () {
			WindowProperties = new PropertyGroup ("Window Properties",
							      typeof (Stetic.Wrapper.Window),
							      "Title",
							      "Icon",
							      "Type",
							      "TypeHint",
							      "WindowPosition",
							      "Modal",
							      "BorderWidth");
			WindowSizeProperties = new PropertyGroup ("Window Size Properties",
								  typeof (Stetic.Wrapper.Window),
								  "Resizable",
								  "AllowGrow",
								  "AllowShrink",
								  "DefaultWidth",
								  "DefaultHeight");
			WindowMiscProperties = new PropertyGroup ("Miscellaneous Window Properties",
								  typeof (Stetic.Wrapper.Window),
								  "AcceptFocus",
								  "Decorated",
								  "DestroyWithParent",
								  "Gravity",
								  "Role",
								  "SkipPagerHint",
								  "SkipTaskbarHint");

			groups = new PropertyGroup[] {
				WindowProperties, WindowSizeProperties, WindowMiscProperties,
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[0];
		}

		public Window () : base ("Window")
		{
			WidgetSite site = new WidgetSite (200, 200);
			site.Show ();
			Add (site);
		}

		public bool HExpandable { get { return true; } }
		public bool VExpandable { get { return true; } }

		public event ExpandabilityChangedHandler ExpandabilityChanged;
	}
}
