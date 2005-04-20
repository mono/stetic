using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Check Menu Item", "checkmenuitem.png", ObjectWrapperType.Internal)]
	public class CheckMenuItem : MenuItem {

		public static new Type WrappedType = typeof (Gtk.CheckMenuItem);

		internal static new void Register (Type type)
		{
			AddItemGroup (type,
				      "Check Menu Item Properties",
				      "Label",
				      "UseUnderline",
				      "Accelerator",
				      "Active",
				      "Inconsistent");
		}
	}
}
