using System;

namespace Stetic.Wrapper {

	public abstract class Misc : Widget {

		public static new Type WrappedType = typeof (Gtk.Misc);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Miscellaneous Alignment Properties",
				      "Xpad",
				      "Ypad",
				      "Xalign",
				      "Yalign");
		}
	}
}
