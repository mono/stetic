using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Event Box", "eventbox.png", ObjectWrapperType.Container)]
	public class EventBox : Bin {

		public static new Type WrappedType = typeof (Gtk.EventBox);

		internal static new void Register (Type type)
		{
			AddItemGroup (type, "Event Box Properties",
				      "AboveChild",
				      "VisibleWindow",
				      "BorderWidth");
		}
	}
}
