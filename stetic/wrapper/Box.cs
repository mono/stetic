using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public static class Box {
		public static PropertyGroup BoxProperties;
		public static PropertyGroup BoxChildProperties;

		static Box () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Box), "Homogeneous"),
				new PropertyDescriptor (typeof (Gtk.Box), "Spacing"),
				new PropertyDescriptor (typeof (Gtk.Container), "BorderWidth"),
			};
			BoxProperties = new PropertyGroup ("Box Properties", props);

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Box.BoxChild), "PackType"),
				new PropertyDescriptor (typeof (Gtk.Box.BoxChild), "Position"),
				new PropertyDescriptor (typeof (Gtk.Box.BoxChild), "Expand"),
				new PropertyDescriptor (typeof (Gtk.Box.BoxChild), "Fill"),
				new PropertyDescriptor (typeof (Gtk.Box.BoxChild), "Padding")
			};
			BoxChildProperties = new PropertyGroup ("Box Child Layout", props);
		}
	}
}
