using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Color Button", "colorbutton.png")]
	public class ColorButton : Gtk.ColorButton, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup ColorButtonProperties;

		static ColorButton () {
			ColorButtonProperties = new PropertyGroup ("Color Button Properties",
								   typeof (Stetic.Wrapper.ColorButton),
								   "Alpha",
								   "Color",
								   "Title",
								   "UseAlpha");

			groups = new PropertyGroup[] {
				ColorButtonProperties,
				Widget.CommonWidgetProperties
			};
		}

		public ColorButton () : base () {}
	}
}
