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

		public static new Gtk.Window CreateInstance ()
		{
			return new Gtk.Window (Gtk.WindowType.Toplevel);
		}

		protected override void Wrap (object obj, bool initialized)
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

		protected override void GladeImport (string className, string id, ArrayList propNames, ArrayList propVals)
		{
			string modal = GladeUtils.ExtractProperty ("modal", propNames, propVals);
			string type_hint = GladeUtils.ExtractProperty ("type_hint", propNames, propVals);
			string type = GladeUtils.ExtractProperty ("type", propNames, propVals);
			base.GladeImport (className, id, propNames, propVals);
			Modal = (modal == "True");

			if (type_hint != null && type_hint.StartsWith ("GDK_WINDOW_TYPE_HINT_"))
				TypeHint = (Gdk.WindowTypeHint) Enum.Parse (typeof (Gdk.WindowTypeHint), type_hint.Substring (21), true);
			if (type != null && type.StartsWith ("GTK_WINDOW_"))
				Type = (Gtk.WindowType) Enum.Parse (typeof (Gtk.WindowType), type.Substring (11), true);
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
		public bool Modal {
			get {
				return modal;
			}
			set {
				modal = value;
			}
		}

		Gdk.WindowTypeHint typeHint;
		public Gdk.WindowTypeHint TypeHint {
			get {
				return typeHint;
			}
			set {
				typeHint = value;
			}
		}

		Gtk.WindowType type;
		public Gtk.WindowType Type {
			get {
				return type;
			}
			set {
				type = value;
			}
		}
	}
}
