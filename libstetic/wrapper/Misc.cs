using System;

namespace Stetic.Wrapper {

	public abstract class Misc : Stetic.Wrapper.Widget {
		public static ItemGroup MiscProperties;

		static Misc () {
			MiscProperties = new ItemGroup ("Miscellaneous Alignment Properties",
							typeof (Gtk.Misc),
							"Xpad",
							"Ypad",
							"Xalign",
							"Yalign");
		}

		protected Misc (IStetic stetic, Gtk.Misc misc, bool initialized) : base (stetic, misc, initialized) {}
	}
}
