using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Combo Box Entry", "comboboxentry.png", ObjectWrapperType.Widget)]
	public class ComboBoxEntry : ComboBox {

		public static new Type WrappedType = typeof (Gtk.ComboBoxEntry);

		public static new Gtk.ComboBoxEntry CreateInstance ()
		{
			return Gtk.ComboBoxEntry.NewText ();
		}
	}
}
