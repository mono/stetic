using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public static class ButtonBox {
		public static PropertyGroup ButtonBoxProperties;

		static ButtonBox () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.ButtonBox), "LayoutStyle"),
				new PropertyDescriptor (typeof (Gtk.Box), "Homogeneous"),
				new PropertyDescriptor (typeof (Gtk.Box), "Spacing"),
				new PropertyDescriptor (typeof (Gtk.Container), "BorderWidth"),
			};
			ButtonBoxProperties = new PropertyGroup ("Button Box Properties", props);
		}
	}
}
