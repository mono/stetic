using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Menu Bar", "menubar.png", ObjectWrapperType.Widget)]
	public class MenuBar : Container {

		public static new Type WrappedType = typeof (Gtk.MenuBar);
	}
}
