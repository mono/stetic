using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public static class Box {
		public static PropertyGroup BoxProperties;

		static Box () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Box), "Homogeneous"),
				new PropertyDescriptor (typeof (Gtk.Box), "Spacing"),
				new PropertyDescriptor (typeof (Gtk.Container), "BorderWidth"),
			};
			BoxProperties = new PropertyGroup ("Box Properties", props);
		}
	}
}
