using System;

namespace Stetic.Editor {

	public class FloatRange : Gtk.SpinButton {

		public FloatRange (double min, double max) : base (min, max, 0.01)
		{
			Digits = 2;
		}
	}
}
