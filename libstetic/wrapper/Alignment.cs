using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Alignment", "alignment.png", ObjectWrapperType.Container)]
	public class Alignment : Bin {
		public static ItemGroup AlignmentProperties;

		static Alignment () {
			AlignmentProperties = new ItemGroup ("Alignment Properties",
							     typeof (Gtk.Alignment),
							     "Xscale",
							     "Yscale",
							     "Xalign",
							     "Yalign",
							     "LeftPadding",
							     "TopPadding",
							     "RightPadding",
							     "BottomPadding",
							     "BorderWidth");
			RegisterWrapper (typeof (Stetic.Wrapper.Alignment),
					 AlignmentProperties,
					 Widget.CommonWidgetProperties);
		}

		public Alignment (IStetic stetic) : this (stetic, new Gtk.Alignment (0.5f, 0.5f, 1.0f, 1.0f)) {}
		public Alignment (IStetic stetic, Gtk.Alignment alignment) : base (stetic, alignment) {}
	}
}
