using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Vertical Scale", "vscale.png", ObjectWrapperType.Widget)]
	public class VScale : Scale {

		public VScale (IStetic stetic) : this (stetic, new Gtk.VScale (0.0, 100.0, 1.0)) {}
		public VScale (IStetic stetic, Gtk.VScale vscale) : base (stetic, vscale) {}
	}
}
