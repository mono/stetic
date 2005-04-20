using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Separator Menu Item", "hseparator.png", ObjectWrapperType.Internal)]
	public class SeparatorMenuItem : Widget {

		public static new Type WrappedType = typeof (Gtk.SeparatorMenuItem);
	}
}
