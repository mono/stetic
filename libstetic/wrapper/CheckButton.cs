using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Check Box", "checkbutton.png", ObjectWrapperType.Widget)]
	public class CheckButton : ToggleButton {

		public static PropertyGroup CheckButtonProperties;

		static CheckButton () {
			CheckButtonProperties = new PropertyGroup ("Check Box Properties",
								   typeof (Gtk.CheckButton),
								   "Label",
								   "Active",
								   "Inconsistent",
								   "DrawIndicator");

			groups = new PropertyGroup[] {
				CheckButtonProperties, Button.ButtonExtraProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public CheckButton (IStetic stetic) : this (stetic, new Gtk.CheckButton ("Check Box")) {}

		public CheckButton (IStetic stetic, Gtk.CheckButton checkbutton) : base (stetic, checkbutton) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
