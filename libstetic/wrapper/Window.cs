using GLib;
using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Window", "window.png", typeof (Gtk.Window), ObjectWrapperType.Window)]
	public class Window : Bin {

		public static ItemGroup WindowProperties;
		public static ItemGroup WindowSizeProperties;
		public static ItemGroup WindowMiscProperties;

		static Window () {
			WindowProperties = new ItemGroup ("Window Properties",
							  typeof (Stetic.Wrapper.Window),
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
			RegisterWrapper (typeof (Stetic.Wrapper.Window),
					 WindowProperties,
					 WindowSizeProperties,
					 WindowMiscProperties,
					 Widget.CommonWidgetProperties);
		}

		public Window (IStetic stetic) : this (stetic, new Gtk.Window (Gtk.WindowType.Toplevel), false) {}


		public Window (IStetic stetic, Gtk.Window window, bool initialized) : base (stetic, window, initialized)
		{
			if (!initialized) {
				window.Title = window.Name;

				if (Site != null) {
					Gtk.Requisition req;
					req.Width = req.Height = 200;
					Site.EmptySize = req;
				}
			}
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

		// We don't want to actually set the underlying "modal" property;
		// that would be annoying to interact with.
		bool modal;
		public bool Modal {
			get {
				return modal;
			}
			set {
				modal = Modal;
			}
		}
	}
}
