using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Statusbar", "statusbar.png", ObjectWrapperType.Widget)]
	public class Statusbar : Stetic.Wrapper.Widget {

		public static ItemGroup StatusbarProperties;

		static Statusbar () {
			StatusbarProperties = new ItemGroup ("Status Bar Properties",
							     typeof (Gtk.Statusbar),
							     "HasResizeGrip");

			groups = new ItemGroup[] {
				StatusbarProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public Statusbar (IStetic stetic) : this (stetic, new Gtk.Statusbar ()) {}

		public Statusbar (IStetic stetic, Gtk.Statusbar statusbar) : base (stetic, statusbar) {}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }
	}
}
