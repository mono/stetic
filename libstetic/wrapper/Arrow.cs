using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Arrow", "arrow.png", ObjectWrapperType.Widget)]
	public class Arrow : Misc {

		public static ItemGroup ArrowProperties;

		static Arrow () {
			ArrowProperties = new ItemGroup ("Arrow Properties",
							 typeof (Gtk.Arrow),
							 "ArrowType",
							 "ShadowType");

			groups = new ItemGroup[] {
				ArrowProperties, Misc.MiscProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public Arrow (IStetic stetic) : this (stetic, new Gtk.Arrow (Gtk.ArrowType.Left, Gtk.ShadowType.None)) {}

		public Arrow (IStetic stetic, Gtk.Arrow arrow) : base (stetic, arrow) {}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }
	}
}
