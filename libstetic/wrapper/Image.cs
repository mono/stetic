using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Image", "image.png", ObjectWrapperType.Widget)]
	public class Image : Misc {

		public static new Type WrappedType = typeof (Gtk.Image);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Image Properties",
				      "File");
		}

		public static new Gtk.Image CreateInstance ()
		{
			return new Gtk.Image ("");
		}

		string filename = "";

		[Editor (typeof (Stetic.Editor.File))]
		public string File {
			get {
				return filename;
			}
			set {
				((Gtk.Image)Wrapped).File = filename = value;
			}
		}
	}
}
