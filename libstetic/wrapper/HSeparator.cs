using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Horizontal Separator", "hseparator.png", ObjectWrapperType.Widget)]
	public class HSeparator : Widget {

		public static new Type WrappedType = typeof (Gtk.HSeparator);

		public override bool HExpandable { get { return true; } }
	}
}
