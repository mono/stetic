using System;
using System.Collections;
using System.ComponentModel;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Image", "image.png", ObjectWrapperType.Widget)]
	public class Image : Misc {

		public static ItemGroup ImageProperties;

		static Image () {
			ImageProperties = new ItemGroup ("Image Properties",
							 typeof (Stetic.Wrapper.Image),
							 typeof (Gtk.Image),
							 "File");

			groups = new ItemGroup[] {
				ImageProperties, Misc.MiscProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public Image (IStetic stetic) : this (stetic, new Gtk.Image ("")) {}

		public Image (IStetic stetic, Gtk.Image image) : base (stetic, image) {}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }

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
