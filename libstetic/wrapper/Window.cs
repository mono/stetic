using GLib;
using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Window", "window.png", ObjectWrapperType.Window)]
	public class Window : Stetic.Wrapper.Container {

		public static ItemGroup WindowProperties;
		public static ItemGroup WindowSizeProperties;
		public static ItemGroup WindowMiscProperties;

		static Window () {
			WindowProperties = new ItemGroup ("Window Properties",
							  typeof (Gtk.Window),
							  "Title",
							  "Icon",
							  "Type",
							  "TypeHint",
							  "WindowPosition",
							  "Modal",
							  "BorderWidth");
			WindowSizeProperties = new ItemGroup ("Window Size Properties",
							      typeof (Gtk.Window),
							      "Resizable",
							      "AllowGrow",
							      "AllowShrink",
							      "DefaultWidth",
							      "DefaultHeight");
			WindowMiscProperties = new ItemGroup ("Miscellaneous Window Properties",
							      typeof (Gtk.Window),
							      "AcceptFocus",
							      "Decorated",
							      "DestroyWithParent",
							      "Gravity",
							      "Role",
							      "SkipPagerHint",
							      "SkipTaskbarHint");

			groups = new ItemGroup[] {
				WindowProperties, WindowSizeProperties, WindowMiscProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new ItemGroup[0];
		}

		public Window (IStetic stetic) : this (stetic, new Gtk.Window ("Window")) {}

		public Window (IStetic stetic, Gtk.Window window) : base (stetic, window)
		{
			window.Add (CreateWidgetSite (200, 200));
			window.DeleteEvent += DeleteEvent;
		}

		[ConnectBefore]
		void DeleteEvent (object obj, Gtk.DeleteEventArgs args)
		{
			((Gtk.Widget)Wrapped).Hide ();
			args.RetVal = true;
		}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }

		static ItemGroup[] childgroups;
		public override ItemGroup[] ChildItemGroups { get { return childgroups; } }

		public override bool HExpandable { get { return true; } }
		public override bool VExpandable { get { return true; } }
	}
}
