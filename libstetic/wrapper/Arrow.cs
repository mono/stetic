using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Arrow", "arrow.png", ObjectWrapperType.Widget)]
	public class Arrow : Misc {

		public static new Type WrappedType = typeof (Gtk.Arrow);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Arrow Properties",
				      "ArrowType",
				      "ShadowType");
		}

		public static new Gtk.Arrow CreateInstance ()
		{
			return new Gtk.Arrow (Gtk.ArrowType.Left, Gtk.ShadowType.None);
		}
	}
}
