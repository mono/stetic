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
			groups = new ItemGroup[] {
				AlignmentProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new ItemGroup[0];
		}

		public Alignment (IStetic stetic) : this (stetic, new Gtk.Alignment (0.5f, 0.5f, 1.0f, 1.0f)) {}

		public Alignment (IStetic stetic, Gtk.Alignment alignment) : base (stetic, alignment) {}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }

		static ItemGroup[] childgroups;
		public override ItemGroup[] ChildItemGroups { get { return childgroups; } }
	}
}
