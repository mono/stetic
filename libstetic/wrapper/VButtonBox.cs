using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("VButtonBox", "vbuttonbox.png", ObjectWrapperType.Container)]
	public class VButtonBox : ButtonBox {

		public VButtonBox (IStetic stetic) : this (stetic, new Gtk.VButtonBox ()) {}
		public VButtonBox (IStetic stetic, Gtk.VButtonBox vbuttonbox) : base (stetic, vbuttonbox) {}

		public override bool HExpandable { get { return false; } }
		public override bool VExpandable { get { return true; } }
	}
}
