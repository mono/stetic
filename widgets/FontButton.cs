using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("Font Button", "fontbutton.png")]
	public class FontButton : Gtk.FontButton, Stetic.IWidgetWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup FontButtonProperties;

		static FontButton () {
			FontButtonProperties = new PropertyGroup ("Font Button Properties",
								  typeof (Stetic.Widget.FontButton),
								  "Title",
								  "FontName",
								  "ShowSize",
								  "ShowStyle",
								  "UseFont",
								  "UseSize");
			FontButtonProperties["UseSize"].DependsOn (FontButtonProperties["UseFont"]);

			groups = new PropertyGroup[] {
				FontButtonProperties,
				Widget.CommonWidgetProperties
			};
		}

		public FontButton (IStetic stetic) : base () {}

		public new bool UseFont {
			get {
				return base.UseFont;
			}
			set {
				base.UseFont = value;

				// Force it to update
				ShowSize = !ShowSize;
				ShowSize = !ShowSize;
			}
		}
	}
}
