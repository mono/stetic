using System;

namespace Stetic.Wrapper {

	public abstract class Misc : Stetic.Wrapper.Widget {
		public static PropertyGroup MiscProperties;

		static Misc () {
			MiscProperties = new PropertyGroup ("Miscellaneous Alignment Properties",
							    typeof (Gtk.Misc),
							    "Xpad",
							    "Ypad",
							    "Xalign",
							    "Yalign");
		}

		protected Misc (IStetic stetic, Gtk.Misc misc) : base (stetic, misc) {}
	}
}
