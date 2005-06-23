using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Font Button", "fontbutton.png", ObjectWrapperType.Widget)]
	public class FontButton : Widget {

		public static new Type WrappedType = typeof (Gtk.FontButton);

		internal static new void Register (Type type)
		{
			ItemGroup props = AddItemGroup (type, "Font Button Properties",
							"Title",
							"FontName",
							"ShowSize",
							"ShowStyle",
							"UseFont",
							"UseSize");
			props["UseSize"].DisabledIf (props["UseFont"], false);
		}

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
