using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Arrow", "arrow.png", typeof (Gtk.Arrow), ObjectWrapperType.Widget)]
	public class Arrow : Misc {

		public static ItemGroup ArrowProperties;

		static Arrow () {
			ArrowProperties = new ItemGroup ("Arrow Properties",
							 typeof (Gtk.Arrow),
							 "ArrowType",
							 "ShadowType");
			RegisterWrapper (typeof (Stetic.Wrapper.Arrow),
					 ArrowProperties,
					 Misc.MiscProperties,
					 Widget.CommonWidgetProperties);
		}

		public Arrow (IStetic stetic) : this (stetic, new Gtk.Arrow (Gtk.ArrowType.Left, Gtk.ShadowType.None), false) {}
		public Arrow (IStetic stetic, Gtk.Arrow arrow, bool initialized) : base (stetic, arrow, initialized) { }
	}
}
