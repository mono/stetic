using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Font Button", "fontbutton.png", ObjectWrapperType.Widget)]
	public class FontButton : Stetic.Wrapper.Widget {

		public static PropertyGroup FontButtonProperties;

		static FontButton () {
			FontButtonProperties = new PropertyGroup ("Font Button Properties",
								  typeof (Stetic.Wrapper.FontButton),
								  typeof (Gtk.FontButton),
								  "Title",
								  "FontName",
								  "ShowSize",
								  "ShowStyle",
								  "UseFont",
								  "UseSize");
			FontButtonProperties["UseSize"].DependsOn (FontButtonProperties["UseFont"]);

			groups = new PropertyGroup[] {
				FontButtonProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public FontButton (IStetic stetic) : this (stetic, new Gtk.FontButton ()) {}

		public FontButton (IStetic stetic, Gtk.FontButton fontbutton) : base (stetic, fontbutton) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }

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
