using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;
using System.ComponentModel;

namespace Stetic.Wrapper {

	public class Image : Gtk.Image, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup ImageProperties;

		static Image () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Stetic.Wrapper.Image), "File"),
			};				
			ImageProperties = new PropertyGroup ("Image Properties", props);

			groups = new PropertyGroup[] {
				ImageProperties, Misc.MiscProperties,
				Widget.CommonWidgetProperties
			};
		}

		public Image () : base ("") {}

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
