using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Editor {

	public class IntRange : Gtk.SpinButton {

		public IntRange (double min, double max, double initial) : base (min, max, 1.0)
		{
			Value = initial;
		}
	}
}
