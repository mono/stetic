using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Radio Button", "radiobutton.png", ObjectWrapperType.Widget)]
	public class RadioButton : ToggleButton {

		public static PropertyGroup RadioButtonProperties;
		public static PropertyGroup RadioButtonExtraProperties;

		static RadioButton () {
			RadioButtonProperties = new PropertyGroup ("Radio Button Properties",
								   typeof (Gtk.RadioButton),
								   "Label",
								   "Active",
								   "Inconsistent",
								   "DrawIndicator");

			groups = new PropertyGroup[] {
				RadioButtonProperties, Button.ButtonExtraProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public RadioButton (IStetic stetic) : this (stetic, new Gtk.RadioButton ("Radio Button")) {}

		public RadioButton (IStetic stetic, Gtk.RadioButton radiobutton) : base (stetic, radiobutton) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
