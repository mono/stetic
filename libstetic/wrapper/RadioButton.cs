using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Radio Button", "radiobutton.png", ObjectWrapperType.Widget)]
	public class RadioButton : ToggleButton {

		public static ItemGroup RadioButtonProperties;
		public static ItemGroup RadioButtonExtraProperties;

		static RadioButton () {
			RadioButtonProperties = new ItemGroup ("Radio Button Properties",
							       typeof (Gtk.RadioButton),
							       "Label",
							       "Active",
							       "Inconsistent",
							       "DrawIndicator");

			groups = new ItemGroup[] {
				RadioButtonProperties, Button.ButtonExtraProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public RadioButton (IStetic stetic) : this (stetic, new Gtk.RadioButton ("")) {}

		public RadioButton (IStetic stetic, Gtk.RadioButton radiobutton) : base (stetic, radiobutton)
		{
			radiobutton.Label = radiobutton.Name;
		}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }
	}
}
