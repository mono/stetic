using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Toolbar Separator", "vseparator.png", ObjectWrapperType.ToolbarItem)]
	public class SeparatorToolItem : Widget {

		public static new Type WrappedType = typeof (Gtk.SeparatorToolItem);

		internal static new void Register (Type type)
		{
			AddItemGroup (type, "Toolbar Separator Properties",
				      "Draw");
		}
	}
}
