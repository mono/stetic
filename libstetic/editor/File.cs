using System;

namespace Stetic.Editor {

	[PropertyEditor ("Filename", "Changed")]
	public class File : Gnome.FileEntry {

		public File () : base ("fileselector", "Select a File") {}
	}
}
