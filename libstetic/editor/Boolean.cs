using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Editor {

	[PropertyEditor ("Active", "Toggled")]
	public class Boolean : Gtk.CheckButton { }
}
