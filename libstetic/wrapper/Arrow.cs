using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Arrow", "arrow.png", ObjectWrapperType.Widget)]
	public class Arrow : Misc {

		public static PropertyGroup ArrowProperties;

		static Arrow () {
			ArrowProperties = new PropertyGroup ("Arrow Properties",
							     typeof (Gtk.Arrow),
							     "ArrowType",
							     "ShadowType");

			groups = new PropertyGroup[] {
				ArrowProperties, Misc.MiscProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public Arrow (IStetic stetic) : this (stetic, new Gtk.Arrow (Gtk.ArrowType.Left, Gtk.ShadowType.None)) {}

		public Arrow (IStetic stetic, Gtk.Arrow arrow) : base (stetic, arrow) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
