using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Vertical Scrollbar", "vscrollbar.png", ObjectWrapperType.Widget)]
	public class VScrollbar : Range {

		public VScrollbar (IStetic stetic) : this (stetic, new Gtk.VScrollbar (new Gtk.Adjustment (0.0, 0.0, 100.0, 1.0, 10.0, 10.0))) {}
		public VScrollbar (IStetic stetic, Gtk.VScrollbar vscrollbar) : base (stetic, vscrollbar) {}

		public override bool VExpandable { get { return true; } }
	}
}
