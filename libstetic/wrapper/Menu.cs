using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Menu", "menu.png", ObjectWrapperType.Internal)]
	public class Menu : Container {

		public static new Type WrappedType = typeof (Gtk.Menu);
	}
}
