using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Alignment", "alignment.png", ObjectWrapperType.Container)]
	public class Alignment : Bin {

		public static new Type WrappedType = typeof (Gtk.Alignment);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Alignment Properties",
				      "Xscale",
				      "Yscale",
				      "Xalign",
				      "Yalign",
				      "LeftPadding",
				      "TopPadding",
				      "RightPadding",
				      "BottomPadding",
				      "BorderWidth");
		}
	}
}
