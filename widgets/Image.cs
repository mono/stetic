using Gtk;
using System;
using System.Collections;
using System.ComponentModel;

namespace Stetic.Widget {

	[WidgetWrapper ("Image", "image.png")]
	public class Image : Gtk.Image, Stetic.IWidgetWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup ImageProperties;

		static Image () {
			ImageProperties = new PropertyGroup ("Image Properties",
							     typeof (Stetic.Widget.Image),
							     "File");

			groups = new PropertyGroup[] {
				ImageProperties, Misc.MiscProperties,
				Widget.CommonWidgetProperties
			};
		}

		public Image (IStetic stetic) : base ("") {}

		string filename = "";

		[Editor (typeof (Stetic.Editor.File), typeof (Gtk.Widget))]
		public new string File {
			get {
				return filename;
			}
			set {
				base.File = filename = value;
			}
		}
	}
}
