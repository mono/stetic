using Gtk;
using System;

namespace Stetic.Editor {

	[PropertyEditor ("Color", "ColorSet")]
	public class Color : Gtk.ColorButton {
		public Color (Gdk.Color color) : base (color) {}
	}
}
