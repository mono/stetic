using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Horizontal Scrollbar", "hscrollbar.png", ObjectWrapperType.Widget)]
	public class HScrollbar : Range {

		public HScrollbar (IStetic stetic) : this (stetic, new Gtk.HScrollbar (new Gtk.Adjustment (0.0, 0.0, 100.0, 1.0, 10.0, 10.0))) {}
		public HScrollbar (IStetic stetic, Gtk.HScrollbar hscrollbar) : base (stetic, hscrollbar) {}

		public override bool HExpandable { get { return true; } }
	}
}
