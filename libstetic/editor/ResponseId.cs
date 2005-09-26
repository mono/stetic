using System;
using System.Collections;
using System.Reflection;

namespace Stetic.Editor {

	public class ResponseId : Gtk.HBox {

		Gtk.ComboBoxEntry combo;
		Gtk.Entry entry;
		EnumDescriptor enm;
		ArrayList values;

		public ResponseId ()
		{
			combo = Gtk.ComboBoxEntry.NewText ();
			combo.Changed += combo_Changed;
			combo.Show ();
			PackStart (combo, true, true, 0);

			entry = combo.Child as Gtk.Entry;
			entry.Changed += entry_Changed;

			enm = Registry.LookupEnum (typeof (Gtk.ResponseType));
			values = new ArrayList ();
			foreach (Enum value in enm.Values) {
				if (enm[value].Label != "") {
					combo.AppendText (enm[value].Label);
					values.Add ((int)enm[value].Value);
				}
			}
		}

		public int Value {
			get {
				if (combo.Active != -1)
					return (int)values[combo.Active];
				else {
					try {
						return Int32.Parse (entry.Text);
					} catch {
						return 0;
					}
				}
			}
			set {
				combo.Active = values.IndexOf (value);
				if (combo.Active == -1)
					entry.Text = value.ToString ();
			}
		}

		public event EventHandler ValueChanged;

		void combo_Changed (object o, EventArgs args)
		{
			if (ValueChanged != null)
				ValueChanged (this, EventArgs.Empty);
		}

		void entry_Changed (object o, EventArgs args)
		{
			if (combo.Active == -1 && ValueChanged != null)
				ValueChanged (this, EventArgs.Empty);
		}
	}
}
