using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Menu", "menu.png", ObjectWrapperType.Widget)]
	public class Menu : Container {

		public static new Type WrappedType = typeof (Gtk.Menu);
	}
}
