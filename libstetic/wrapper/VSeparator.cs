using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Vertical Separator", "vseparator.png", ObjectWrapperType.Widget)]
	public class VSeparator : Stetic.Wrapper.Widget {

		public VSeparator (IStetic stetic) : this (stetic, new Gtk.VSeparator ()) {}
		public VSeparator (IStetic stetic, Gtk.VSeparator vseparator) : base (stetic, vseparator) {}

		public override bool VExpandable { get { return true; } }
	}
}
