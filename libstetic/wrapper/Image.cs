using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Image", "image.png", ObjectWrapperType.Widget)]
	public class Image : Misc {

		public static ItemGroup ImageProperties;

		static Image () {
			ImageProperties = new ItemGroup ("Image Properties",
							 typeof (Stetic.Wrapper.Image),
							 typeof (Gtk.Image),
							 "File");
			RegisterWrapper (typeof (Stetic.Wrapper.Image),
					 ImageProperties,
					 Misc.MiscProperties,
					 Widget.CommonWidgetProperties);
		}

		public Image (IStetic stetic) : this (stetic, new Gtk.Image ("")) {}
		public Image (IStetic stetic, Gtk.Image image) : base (stetic, image) {}

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
