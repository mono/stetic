using GLib;
using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Window", "window.png", ObjectWrapperType.Window)]
	public class Window : Bin {

		public static new Type WrappedType = typeof (Gtk.Window);

		static new void Register (Type type)
		{
			if (type == typeof (Stetic.Wrapper.Window)) {
				AddItemGroup (type, "Window Properties",
					      "Title",
					      "Icon",
					      "Type",
					      "TypeHint",
					      "WindowPosition",
					      "Modal",
					      "BorderWidth");
			}
			AddItemGroup (type, "Window Size Properties",
				      "Resizable",
				      "AllowGrow",
				      "AllowShrink",
				      "DefaultWidth",
				      "DefaultHeight");
			AddItemGroup (type, "Miscellaneous Window Properties",
				      "AcceptFocus",
				      "Decorated",
				      "DestroyWithParent",
				      "Gravity",
				      "Role",
				      "SkipPagerHint",
				      "SkipTaskbarHint");
		}

		public static new Gtk.Window CreateInstance ()
		{
			return new Gtk.Window (Gtk.WindowType.Toplevel);
		}

		protected override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);

			Gtk.Window window = (Gtk.Window)Wrapped;
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
