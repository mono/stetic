using GLib;
using System;
using System.Collections;

namespace Stetic.Wrapper {

	public class Window : Bin {

		public override void Wrap (object obj, bool initialized)
		{
			Gtk.Window window = (Gtk.Window)obj;

			window.TypeHint = Gdk.WindowTypeHint.Normal;
			base.Wrap (obj, initialized);

			if (!initialized) {
				window.Title = window.Name;

				if (window.Child is Placeholder)
					window.Child.SetSizeRequest (200, 200);
			}

			window.DeleteEvent += DeleteEvent;
		}

		public override void Select (Stetic.Wrapper.Widget wrapper)
		{
			if (wrapper == this)
				Wrapped.Show ();
			base.Select (wrapper);
		}

		[ConnectBefore]
		void DeleteEvent (object obj, Gtk.DeleteEventArgs args)
		{
			Wrapped.Hide ();
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
