using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("HButtonBox", "hbuttonbox.png", ObjectWrapperType.Container)]
	public class HButtonBox : ButtonBox {

		public static new Type WrappedType = typeof (Gtk.HButtonBox);

		public override bool HExpandable { get { return true; } }
	}
}
