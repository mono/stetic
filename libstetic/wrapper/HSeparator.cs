using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Horizontal Separator", "hseparator.png", ObjectWrapperType.Widget)]
	public class HSeparator : Stetic.Wrapper.Widget {

		public HSeparator (IStetic stetic) : this (stetic, new Gtk.HSeparator ()) {}
		public HSeparator (IStetic stetic, Gtk.HSeparator hseparator) : base (stetic, hseparator) {}

		public override bool HExpandable { get { return true; } }
	}
}
