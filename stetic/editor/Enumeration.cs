using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;

namespace Stetic.Editor {

	public class Enumeration : Gtk.HBox {

		ComboBox combo;
		ArrayList values;

		public Enumeration (Type type) : base (false, 0)
		{
			combo = ComboBox.NewText ();
			combo.Changed += combo_Changed;
			combo.Show ();
			PackStart (combo, true, true, 0);

			values = new ArrayList ();
			foreach (int i in Enum.GetValues (type)) {
				Enum value = (Enum)Enum.ToObject (type, i);
				string name = Enum.GetName (type, value);

				combo.AppendText (name + " (" + i.ToString() + ")");
				values.Add (value);
			}
		}

		public Enum Value {
			get {
				return (Enum)values[combo.Active];
			}
			set {
				int i = values.IndexOf (value);
				if (i != -1)
					combo.Active = i;
			}
		}

		public event EventHandler ValueChanged;

		void combo_Changed (object o, EventArgs args)
		{
			if (ValueChanged != null)
				ValueChanged (this, EventArgs.Empty);
		}
	}
}
