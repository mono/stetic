using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Toolbar Toggle Button", "checkbutton.png", ObjectWrapperType.ToolbarItem)]
	public class ToggleToolButton : ToolButton {

		public static new Type WrappedType = typeof (Gtk.ToggleToolButton);

		internal static new void Register (Type type)
		{
			AddItemGroup (type, "Toolbar Toggle Button Properties",
				      "Icon",
				      "Label",
				      "UseUnderline",
				      "Active");
		}

		public static new Gtk.ToolButton CreateInstance ()
		{
			return new Gtk.ToggleToolButton (Gtk.Stock.Bold);
		}
	}
}
