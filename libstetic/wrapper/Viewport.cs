using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Viewport", "viewport.png", ObjectWrapperType.Internal)]
	public class Viewport : Bin {

		public static new Type WrappedType = typeof (Gtk.Viewport);

		internal static new void Register (Type type)
		{
			AddItemGroup (type, "Viewport Properties",
				      "ShadowType");
		}

		public Viewport ()
		{
			Unselectable = true;
		}
	}
}
