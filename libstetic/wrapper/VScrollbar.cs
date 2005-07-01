using System;

namespace Stetic.Wrapper {

	public class VScrollbar : Range {

		public static new Gtk.VScrollbar CreateInstance ()
		{
			return new Gtk.VScrollbar (new Gtk.Adjustment (0.0, 0.0, 100.0, 1.0, 10.0, 10.0));
		}
	}
}
