using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Editor {

	[PropertyEditor ("Filename", "Changed")]
	public class File : Gnome.FileEntry {

		public File (string value) : base ("fileselector", "Select a File")
		{
			if (value != null)
				Filename = value;
		}
	}
}
