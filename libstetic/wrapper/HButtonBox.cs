using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("HButtonBox", "hbuttonbox.png", ObjectWrapperType.Container)]
	public class HButtonBox : ButtonBox {

		public HButtonBox (IStetic stetic) : this (stetic, new Gtk.HButtonBox ()) {}
		public HButtonBox (IStetic stetic, Gtk.HButtonBox hbuttonbox) : base (stetic, hbuttonbox) {}

		public override bool HExpandable { get { return true; } }
	}
}
