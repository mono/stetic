using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Vertical Scrollbar", "vscrollbar.png", ObjectWrapperType.Widget)]
	public class VScrollbar : Range {

		public static new Type WrappedType = typeof (Gtk.VScrollbar);

		public static new Gtk.VScrollbar CreateInstance ()
		{
			return new Gtk.VScrollbar (new Gtk.Adjustment (0.0, 0.0, 100.0, 1.0, 10.0, 10.0));
		}

		public override bool VExpandable { get { return true; } }
	}
}
