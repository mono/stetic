using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Editor {

	public class FloatRange : Gtk.SpinButton {

		public FloatRange (double min, double max, double initial) : base (min, max, 0.01)
		{
			Value = initial;
		}
	}
}
