using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Horizontal Scale", "hscale.png", ObjectWrapperType.Widget)]
	public class HScale : Scale {

		public HScale (IStetic stetic) : this (stetic, new Gtk.HScale (0.0, 100.0, 1.0)) {}
		public HScale (IStetic stetic, Gtk.HScale hscale) : base (stetic, hscale) {}

		public override bool HExpandable { get { return true; } }
	}
}
