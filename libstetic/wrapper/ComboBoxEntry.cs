using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Combo Box Entry", "comboboxentry.png", ObjectWrapperType.Widget)]
	public class ComboBoxEntry : Stetic.Wrapper.ComboBox {

		static ComboBoxEntry () {
			groups = new ItemGroup[] {
				Stetic.Wrapper.ComboBox.ComboBoxProperties,
				Stetic.Wrapper.ComboBox.ComboBoxExtraProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public ComboBoxEntry (IStetic stetic) : this (stetic, Gtk.ComboBoxEntry.NewText ()) {}

		public ComboBoxEntry (IStetic stetic, Gtk.ComboBoxEntry comboboxentry) : base (stetic, comboboxentry) {}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }
	}
}
