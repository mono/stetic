using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public static class Misc {
		public static PropertyGroup MiscProperties;

		static Misc () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Misc), "Xpad"),
				new PropertyDescriptor (typeof (Gtk.Misc), "Ypad"),
				new PropertyDescriptor (typeof (Gtk.Misc), "Xalign"),
				new PropertyDescriptor (typeof (Gtk.Misc), "Yalign"),
			};
			MiscProperties = new PropertyGroup ("Miscellaneous Alignment Properties", props);
		}
	}
}
