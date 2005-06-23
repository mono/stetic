using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Toggle Button", "togglebutton.png", ObjectWrapperType.Widget)]
	public class ToggleButton : Button {

		public static new Type WrappedType = typeof (Gtk.ToggleButton);

		internal static new void Register (Type type)
		{
			AddItemGroup (type, "Toggle Button Properties",
				      "Type",
				      "StockId",
				      "ApplicationIcon",
				      "ThemedIcon",
				      "Label",
				      "UseUnderline",
				      "Active",
				      "Inconsistent");
		}
	}
}
