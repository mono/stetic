using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Alignment", "alignment.png", ObjectWrapperType.Container)]
	public class Alignment : Bin {
		public static PropertyGroup AlignmentProperties;

		static Alignment () {
			AlignmentProperties = new PropertyGroup ("Alignment Properties",
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
			groups = new PropertyGroup[] {
				AlignmentProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[0];
		}

		public Alignment (IStetic stetic) : this (stetic, new Gtk.Alignment (0.5f, 0.5f, 1.0f, 1.0f)) {}

		public Alignment (IStetic stetic, Gtk.Alignment alignment) : base (stetic, alignment) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public override PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }
	}
}
