using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("Combo Box", "combo.png")]
	public class ComboBox : Gtk.ComboBox, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup ComboBoxProperties;
		public static PropertyGroup ComboBoxExtraProperties;

		static ComboBox () {
			ComboBoxProperties = new PropertyGroup ("Combo Box Properties",
								typeof (Stetic.Widget.ComboBox),
								"Model",
								"Active");
			ComboBoxExtraProperties = new PropertyGroup ("Extra ComboBox Properties",
								     typeof (Stetic.Widget.ComboBox),
								     "WrapWidth",
								     "ColumnSpanColumn",
								     "RowSpanColumn");

			groups = new PropertyGroup[] {
				ComboBoxProperties, ComboBoxExtraProperties,
				Widget.CommonWidgetProperties
			};
		}

		public ComboBox (IStetic stetic) : base () {}
	}
}
