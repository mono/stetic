using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Vertical Separator", "vseparator.png", ObjectWrapperType.Widget)]
	public class VSeparator : Widget {

		public static new Type WrappedType = typeof (Gtk.VSeparator);

		public override bool VExpandable { get { return true; } }
	}
}
