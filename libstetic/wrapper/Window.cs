using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Window", "window.png", ObjectWrapperType.Window)]
	public class Window : Stetic.Wrapper.Container {

		public static PropertyGroup WindowProperties;
		public static PropertyGroup WindowSizeProperties;
		public static PropertyGroup WindowMiscProperties;

		static Window () {
			WindowProperties = new PropertyGroup ("Window Properties",
							      typeof (Gtk.Window),
							      "Title",
							      "Icon",
							      "Type",
							      "TypeHint",
							      "WindowPosition",
							      "Modal",
							      "BorderWidth");
			WindowSizeProperties = new PropertyGroup ("Window Size Properties",
								  typeof (Gtk.Window),
								  "Resizable",
								  "AllowGrow",
								  "AllowShrink",
								  "DefaultWidth",
								  "DefaultHeight");
			WindowMiscProperties = new PropertyGroup ("Miscellaneous Window Properties",
								  typeof (Gtk.Window),
								  "AcceptFocus",
								  "Decorated",
								  "DestroyWithParent",
								  "Gravity",
								  "Role",
								  "SkipPagerHint",
								  "SkipTaskbarHint");

			groups = new PropertyGroup[] {
				WindowProperties, WindowSizeProperties, WindowMiscProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[0];
		}

		public Window (IStetic stetic) : this (stetic, new Gtk.Window ("Window")) {}

		public Window (IStetic stetic, Gtk.Window window) : base (stetic, window)
		{
			window.Add (CreateWidgetSite (200, 200));
		}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public override PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		public override bool HExpandable { get { return true; } }
		public override bool VExpandable { get { return true; } }
	}
}
