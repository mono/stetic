using System;

namespace Stetic.Editor {

	public class FloatRange : Gtk.SpinButton {

		public FloatRange (object min, object max) :
			base (min == null ? Double.MinValue : (double)min,
			      max == null ? Double.MaxValue : (double)max,
			      0.01)
		{
			Digits = 2;
		}
	}
}
