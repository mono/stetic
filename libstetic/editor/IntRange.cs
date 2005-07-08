using System;

namespace Stetic.Editor {

	public class IntRange : Gtk.SpinButton {

		public IntRange (object min, object max) :
			base (min == null ? (double)Int32.MinValue : (double)min,
			      max == null ? (double)Int32.MaxValue : (double)max,
			      1.0) {}
	}
}
