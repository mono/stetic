using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Horizontal Scale", "hscale.png", typeof (Gtk.HScale), ObjectWrapperType.Widget)]
	public class HScale : Scale {

		public HScale (IStetic stetic) : this (stetic, new Gtk.HScale (0.0, 100.0, 1.0), false) {}
		public HScale (IStetic stetic, Gtk.HScale hscale, bool initialized) : base (stetic, hscale, initialized) {}

		public override bool HExpandable { get { return true; } }
	}
}
