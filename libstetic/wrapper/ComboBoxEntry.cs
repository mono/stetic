using System;

namespace Stetic.Wrapper {

	public class ComboBoxEntry : ComboBox {

		public static new Gtk.ComboBoxEntry CreateInstance ()
		{
			return Gtk.ComboBoxEntry.NewText ();
		}
	}
}
