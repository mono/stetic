using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("Radio Button", "radiobutton.png")]
	public class RadioButton : Gtk.RadioButton, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup RadioButtonProperties;
		public static PropertyGroup RadioButtonExtraProperties;

		static RadioButton () {
			RadioButtonProperties = new PropertyGroup ("Radio Button Properties",
								   typeof (Stetic.Widget.RadioButton),
								   "Label",
								   "Active",
								   "Inconsistent",
								   "DrawIndicator");

			groups = new PropertyGroup[] {
				RadioButtonProperties, Button.ButtonExtraProperties,
				Widget.CommonWidgetProperties
			};
		}

		public RadioButton (IStetic stetic) : base ("Radio Button") {}
	}
}
