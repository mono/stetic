using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Color Button", "colorbutton.png", ObjectWrapperType.Widget)]
	public class ColorButton : Stetic.Wrapper.Widget {

		public static ItemGroup ColorButtonProperties;

		static ColorButton () {
			// FIXME: we should merge UseAlpha and Alpha into an OptIntRange

			ColorButtonProperties = new ItemGroup ("Color Button Properties",
							       typeof (Gtk.ColorButton),
							       "Title",
							       "Color",
							       "UseAlpha",
							       "Alpha");
			ColorButtonProperties["Alpha"].DependsOn (ColorButtonProperties["UseAlpha"]);

			groups = new ItemGroup[] {
				ColorButtonProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public ColorButton (IStetic stetic) : this (stetic, new Gtk.ColorButton ()) {}

		public ColorButton (IStetic stetic, Gtk.ColorButton colorbutton) : base (stetic, colorbutton) {}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }
	}
}
