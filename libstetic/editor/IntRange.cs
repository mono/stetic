using System;

namespace Stetic.Editor {

	public class IntRange : Gtk.SpinButton {

		public IntRange (double min, double max) : base (min, max, 1.0) {}
	}
}
