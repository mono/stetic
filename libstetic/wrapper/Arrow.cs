using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Arrow", "arrow.png", ObjectWrapperType.Widget)]
	public class Arrow : Misc {

		public static new Type WrappedType = typeof (Gtk.Arrow);

		internal static new void Register (Type type)
		{
			AddItemGroup (type, "Arrow Properties",
				      "ArrowType",
				      "ShadowType");
		}
	}
}
