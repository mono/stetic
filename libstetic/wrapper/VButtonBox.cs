using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("VButtonBox", "vbuttonbox.png", typeof (Gtk.VButtonBox), ObjectWrapperType.Container)]
	public class VButtonBox : ButtonBox {

		public VButtonBox (IStetic stetic) : this (stetic, new Gtk.VButtonBox (), false) {}
		public VButtonBox (IStetic stetic, Gtk.VButtonBox vbuttonbox, bool initialized) : base (stetic, vbuttonbox, initialized) {}

		public override bool VExpandable { get { return true; } }
	}
}
