using System;
using System.Collections;
using System.ComponentModel;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Image", "image.png", ObjectWrapperType.Widget)]
	public class Image : Misc {

		public static PropertyGroup ImageProperties;

		static Image () {
			ImageProperties = new PropertyGroup ("Image Properties",
							     typeof (Stetic.Wrapper.Image),
							     typeof (Gtk.Image),
							     "File");

			groups = new PropertyGroup[] {
				ImageProperties, Misc.MiscProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public Image (IStetic stetic) : this (stetic, new Gtk.Image ("")) {}

		public Image (IStetic stetic, Gtk.Image image) : base (stetic, image) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }

		string filename = "";

		[Editor (typeof (Stetic.Editor.File), typeof (Gtk.Widget))]
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
