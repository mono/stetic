using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Combo Box Entry", "comboboxentry.png", typeof (Gtk.ComboBoxEntry), ObjectWrapperType.Widget)]
	public class ComboBoxEntry : Stetic.Wrapper.ComboBox {

		static ComboBoxEntry () {
			RegisterWrapper (typeof (Stetic.Wrapper.ComboBoxEntry),
					 ComboBox.ComboBoxProperties,
					 ComboBox.ComboBoxExtraProperties,
					 Widget.CommonWidgetProperties);
		}

		public ComboBoxEntry (IStetic stetic) : this (stetic, Gtk.ComboBoxEntry.NewText (), false) {}
		public ComboBoxEntry (IStetic stetic, Gtk.ComboBoxEntry comboboxentry, bool initialized) : base (stetic, comboboxentry, initialized) {}
	}
}
