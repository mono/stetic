using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Horizontal Scrollbar", "hscrollbar.png", ObjectWrapperType.Widget)]
	public class HScrollbar : Range {

		public static new Type WrappedType = typeof (Gtk.HScrollbar);

		public static new Gtk.HScrollbar CreateInstance ()
		{
			return new Gtk.HScrollbar (new Gtk.Adjustment (0.0, 0.0, 100.0, 1.0, 10.0, 10.0));
		}

		public override bool HExpandable { get { return true; } }
	}
}
