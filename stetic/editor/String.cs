using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Editor {

	[PropertyEditor ("Text", "Changed")]
	public class String : Gtk.Entry {

		public String (string value)
		{
			if (value != null)
				Text = value;
		}
	}
}
