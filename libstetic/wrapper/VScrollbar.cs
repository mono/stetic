using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Vertical Scrollbar", "vscrollbar.png", typeof (Gtk.VScrollbar), ObjectWrapperType.Widget)]
	public class VScrollbar : Range {

		public VScrollbar (IStetic stetic) : this (stetic, new Gtk.VScrollbar (new Gtk.Adjustment (0.0, 0.0, 100.0, 1.0, 10.0, 10.0)), false) {}
		public VScrollbar (IStetic stetic, Gtk.VScrollbar vscrollbar, bool initialized) : base (stetic, vscrollbar, initialized) {}

		public override bool VExpandable { get { return true; } }
	}
}
