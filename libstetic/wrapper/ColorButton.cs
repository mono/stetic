using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Color Button", "colorbutton.png", ObjectWrapperType.Widget)]
	public class ColorButton : Widget {

		public static new Type WrappedType = typeof (Gtk.ColorButton);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Color Button Properties",
				      "Title",
				      "Color",
				      "Alpha");
		}

		[Range (-1, 65535)]
		public int Alpha {
			get {
				Gtk.ColorButton cb = (Gtk.ColorButton)Wrapped;

				if (cb.UseAlpha)
					return cb.Alpha;
				else
					return -1;
			}
			set {
				Gtk.ColorButton cb = (Gtk.ColorButton)Wrapped;

				if (value == -1)
					cb.UseAlpha = false;
				else {
					cb.UseAlpha = true;
					cb.Alpha = (ushort)value;
				}
			}
		}
	}
}
