using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Color Button", "colorbutton.png", ObjectWrapperType.Widget)]
	public class ColorButton : Stetic.Wrapper.Widget {

		public static PropertyGroup ColorButtonProperties;

		static ColorButton () {
			ColorButtonProperties = new PropertyGroup ("Color Button Properties",
								   typeof (Gtk.ColorButton),
								   "Alpha",
								   "Color",
								   "Title",
								   "UseAlpha");

			groups = new PropertyGroup[] {
				ColorButtonProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public ColorButton (IStetic stetic) : this (stetic, new Gtk.ColorButton ()) {}

		public ColorButton (IStetic stetic, Gtk.ColorButton colorbutton) : base (stetic, colorbutton) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
