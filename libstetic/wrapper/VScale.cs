using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Vertical Scale", "vscale.png", typeof (Gtk.VScale), ObjectWrapperType.Widget)]
	public class VScale : Scale {

		public VScale (IStetic stetic) : this (stetic, new Gtk.VScale (0.0, 100.0, 1.0), false) {}
		public VScale (IStetic stetic, Gtk.VScale vscale, bool initialized) : base (stetic, vscale, initialized) {}

		public override bool VExpandable { get { return true; } }
	}
}
