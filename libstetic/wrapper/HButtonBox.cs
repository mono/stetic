using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("HButtonBox", "hbuttonbox.png", typeof (Gtk.HButtonBox), ObjectWrapperType.Container)]
	public class HButtonBox : ButtonBox {

		public HButtonBox (IStetic stetic) : this (stetic, new Gtk.HButtonBox (), false) {}
		public HButtonBox (IStetic stetic, Gtk.HButtonBox hbuttonbox, bool initialized) : base (stetic, hbuttonbox, initialized) {}

		public override bool HExpandable { get { return true; } }
	}
}
