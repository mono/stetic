using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Radio Button", "radiobutton.png")]
	public class RadioButton : Gtk.RadioButton, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup RadioButtonProperties;
		public static PropertyGroup RadioButtonExtraProperties;

		static RadioButton () {
			RadioButtonProperties = new PropertyGroup ("Radio Button Properties",
								   typeof (Stetic.Wrapper.RadioButton),
								   "Label",
								   "Active",
								   "Inconsistent",
								   "DrawIndicator");

			groups = new PropertyGroup[] {
				RadioButtonProperties, Button.ButtonExtraProperties,
				Widget.CommonWidgetProperties
			};
		}

		public RadioButton () : base ("Radio Button") {}
	}
}
