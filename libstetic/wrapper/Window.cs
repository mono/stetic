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
			RegisterItems (typeof (Stetic.Wrapper.Window),
				       WindowProperties,
				       WindowSizeProperties,
				       WindowMiscProperties,
				       Widget.CommonWidgetProperties);
		}

		public Window (IStetic stetic) : this (stetic, new Gtk.Window (Gtk.WindowType.Toplevel)) {}

		public Window (IStetic stetic, Gtk.Window window) : base (stetic, window)
		{
			window.Title = window.Name;
			window.Add (CreateWidgetSite (200, 200));
			window.DeleteEvent += DeleteEvent;
		}

		[ConnectBefore]
		void DeleteEvent (object obj, Gtk.DeleteEventArgs args)
		{
			((Gtk.Widget)Wrapped).Hide ();
			args.RetVal = true;
		}

		public override bool HExpandable { get { return true; } }
		public override bool VExpandable { get { return true; } }
	}
}
