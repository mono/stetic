using GLib;
using System;
using System.Collections;

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

		public override void Wrap (object obj, bool initialized)
		{
			Gtk.Window window = (Gtk.Window)obj;

			window.TypeHint = Gdk.WindowTypeHint.Normal;
			base.Wrap (obj, initialized);

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

		// We don't want to actually set the underlying properties for these;
		// that would be annoying to interact with.
		bool modal;
		[GladeProperty]
		public bool Modal {
			get {
				return modal;
			}
			set {
				modal = value;
			}
		}

		Gdk.WindowTypeHint typeHint;
		[GladeProperty]
		public Gdk.WindowTypeHint TypeHint {
			get {
				return typeHint;
			}
			set {
				typeHint = value;
			}
		}

		Gtk.WindowType type;
		[GladeProperty]
		public Gtk.WindowType Type {
			get {
				return type;
			}
			set {
				type = value;
			}
		}

		string icon;
		[Editor (typeof (Stetic.Editor.ImageFile))]
		public string Icon {
			get {
				return icon;
			}
			set {
				icon = value;
				Gtk.Window window = (Gtk.Window)Wrapped;
				try {
					window.Icon = new Gdk.Pixbuf (icon);
				} catch {
					window.Icon = null;
				}
			}
		}
	}
}
