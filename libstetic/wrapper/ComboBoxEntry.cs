using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Combo Box Entry", "comboboxentry.png", ObjectWrapperType.Widget)]
	public class ComboBoxEntry : Stetic.Wrapper.ComboBox {

		static ComboBoxEntry () {
			RegisterWrapper (typeof (Stetic.Wrapper.ComboBoxEntry),
					 ComboBox.ComboBoxProperties,
					 ComboBox.ComboBoxExtraProperties,
					 Widget.CommonWidgetProperties);
		}

		public ComboBoxEntry (IStetic stetic) : this (stetic, Gtk.ComboBoxEntry.NewText ()) {}
		public ComboBoxEntry (IStetic stetic, Gtk.ComboBoxEntry comboboxentry) : base (stetic, comboboxentry) {}
	}
}
