using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Statusbar", "statusbar.png", ObjectWrapperType.Widget)]
	public class Statusbar : Stetic.Wrapper.Widget {

		public static PropertyGroup StatusbarProperties;

		static Statusbar () {
			StatusbarProperties = new PropertyGroup ("Status Bar Properties",
								 typeof (Gtk.Statusbar),
								 "HasResizeGrip");

			groups = new PropertyGroup[] {
				StatusbarProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public Statusbar (IStetic stetic) : this (stetic, new Gtk.Statusbar ()) {}

		public Statusbar (IStetic stetic, Gtk.Statusbar statusbar) : base (stetic, statusbar) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
