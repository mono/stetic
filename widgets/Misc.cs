using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public static class Misc {
		public static PropertyGroup MiscProperties;

		static Misc () {
			MiscProperties = new PropertyGroup ("Miscellaneous Alignment Properties",
							    typeof (Gtk.Misc),
							    "Xpad",
							    "Ypad",
							    "Xalign",
							    "Yalign");
		}
	}
}
