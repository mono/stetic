using Gtk;
using System;

namespace Stetic.Widget {

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
