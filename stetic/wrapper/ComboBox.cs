using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class ComboBox : Gtk.ComboBox, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup ComboBoxProperties;
		public static PropertyGroup ComboBoxExtraProperties;

		static ComboBox () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.ComboBox), "Model"),
				new PropertyDescriptor (typeof (Gtk.ComboBox), "Active"),
			};				
			ComboBoxProperties = new PropertyGroup ("ComboBox Properties", props);

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.ComboBox), "WrapWidth"),
				new PropertyDescriptor (typeof (Gtk.ComboBox), "ColumnSpanColumn"),
				new PropertyDescriptor (typeof (Gtk.ComboBox), "RowSpanColumn"),
			};
			ComboBoxExtraProperties = new PropertyGroup ("Extra ComboBox Properties", props);

			groups = new PropertyGroup[] {
				ComboBoxProperties, ComboBoxExtraProperties,
				Widget.CommonWidgetProperties
			};
		}

		public ComboBox () : base () {}
	}
}
