using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Horizontal Separator", "hseparator.png", typeof (Gtk.HSeparator), ObjectWrapperType.Widget)]
	public class HSeparator : Stetic.Wrapper.Widget {

		public HSeparator (IStetic stetic) : this (stetic, new Gtk.HSeparator (), false) {}
		public HSeparator (IStetic stetic, Gtk.HSeparator hseparator, bool initialized) : base (stetic, hseparator, initialized) {}

		public override bool HExpandable { get { return true; } }
	}
}
