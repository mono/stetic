using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Font Button", "fontbutton.png", ObjectWrapperType.Widget)]
	public class FontButton : Stetic.Wrapper.Widget {

		public static ItemGroup FontButtonProperties;

		static FontButton () {
			FontButtonProperties = new ItemGroup ("Font Button Properties",
							      typeof (Stetic.Wrapper.FontButton),
							      typeof (Gtk.FontButton),
							      "Title",
							      "FontName",
							      "ShowSize",
							      "ShowStyle",
							      "UseFont",
							      "UseSize");
			FontButtonProperties["UseSize"].DependsOn (FontButtonProperties["UseFont"]);

			RegisterItems (typeof (Stetic.Wrapper.FontButton),
				       FontButtonProperties,
				       Widget.CommonWidgetProperties);
		}

		public FontButton (IStetic stetic) : this (stetic, new Gtk.FontButton ()) {}
		public FontButton (IStetic stetic, Gtk.FontButton fontbutton) : base (stetic, fontbutton) {}

		public bool UseFont {
			get {
				return ((Gtk.FontButton)Wrapped).UseFont;
			}
			set {
				Gtk.FontButton fb = (Gtk.FontButton)Wrapped;

				fb.UseFont = value;

				// Force it to update
				fb.ShowSize = !fb.ShowSize;
				fb.ShowSize = !fb.ShowSize;
			}
		}
	}
}
