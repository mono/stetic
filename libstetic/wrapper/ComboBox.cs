using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Combo Box", "combo.png", ObjectWrapperType.Widget)]
	public class ComboBox : Stetic.Wrapper.Widget {

		public static PropertyGroup ComboBoxProperties;
		public static PropertyGroup ComboBoxExtraProperties;

		static ComboBox () {
			ComboBoxProperties = new PropertyGroup ("Combo Box Properties",
								typeof (Gtk.ComboBox),
								"Model",
								"Active");
			ComboBoxExtraProperties = new PropertyGroup ("Extra ComboBox Properties",
								     typeof (Gtk.ComboBox),
								     "WrapWidth",
								     "ColumnSpanColumn",
								     "RowSpanColumn");

			groups = new PropertyGroup[] {
				ComboBoxProperties, ComboBoxExtraProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public ComboBox (IStetic stetic) : this (stetic, new Gtk.ComboBox ()) {}

		public ComboBox (IStetic stetic, Gtk.ComboBox combobox) : base (stetic, combobox) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
