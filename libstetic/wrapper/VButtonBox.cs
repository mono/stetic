using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("VButtonBox", "vbuttonbox.png", ObjectWrapperType.Container)]
	public class VButtonBox : ButtonBox {

		public static new Type WrappedType = typeof (Gtk.VButtonBox);

		public override bool VExpandable { get { return true; } }
	}
}
