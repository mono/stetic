using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Color Button", "colorbutton.png", typeof (Gtk.ColorButton), ObjectWrapperType.Widget)]
	public class ColorButton : Stetic.Wrapper.Widget {

		public static ItemGroup ColorButtonProperties;

		static ColorButton () {
			ColorButtonProperties = new ItemGroup ("Color Button Properties",
							       typeof (Stetic.Wrapper.ColorButton),
							       typeof (Gtk.ColorButton),
							       "Title",
							       "Color",
							       "Alpha");
			RegisterWrapper (typeof (Stetic.Wrapper.ColorButton),
					 ColorButtonProperties,
					 Widget.CommonWidgetProperties);
		}

		public ColorButton (IStetic stetic) : this (stetic, new Gtk.ColorButton (), false) {}
		public ColorButton (IStetic stetic, Gtk.ColorButton colorbutton, bool initialized) : base (stetic, colorbutton, initialized) {}

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
