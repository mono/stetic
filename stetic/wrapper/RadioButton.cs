using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class RadioButton : Gtk.RadioButton, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup RadioButtonProperties;
		public static PropertyGroup RadioButtonExtraProperties;

		static RadioButton () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.ToggleButton), "Active"),
				new PropertyDescriptor (typeof (Gtk.ToggleButton), "Inconsistent"),
				new PropertyDescriptor (typeof (Gtk.ToggleButton), "DrawIndicator"),
			};				
			RadioButtonProperties = new PropertyGroup ("Radio Button Properties", props);

			groups = new PropertyGroup[] {
				RadioButtonProperties, Widget.CommonWidgetProperties
			};
		}

		public RadioButton () : base ("Radio Button") {}
	}
}
