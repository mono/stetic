using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Vertical Separator", "vseparator.png", typeof (Gtk.VSeparator), ObjectWrapperType.Widget)]
	public class VSeparator : Stetic.Wrapper.Widget {

		public VSeparator (IStetic stetic) : this (stetic, new Gtk.VSeparator (), false) {}
		public VSeparator (IStetic stetic, Gtk.VSeparator vseparator, bool initialized) : base (stetic, vseparator, initialized) {}

		public override bool VExpandable { get { return true; } }
	}
}
