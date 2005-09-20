using System;
using System.Collections;

namespace Stetic.Editor {

	public class Enumeration : Gtk.HBox {

		Gtk.EventBox ebox;
		Gtk.ComboBox combo;
		Gtk.Tooltips tips;
		EnumDescriptor enm;

		public Enumeration (PropertyDescriptor prop) : base (false, 0)
		{
			ebox = new Gtk.EventBox ();
			ebox.Show ();
			PackStart (ebox, true, true, 0);

			combo = Gtk.ComboBox.NewText ();
			combo.Changed += combo_Changed;
			combo.Show ();
			ebox.Add (combo);

			tips = new Gtk.Tooltips ();

			enm = Registry.LookupEnum (prop.PropertyType);
			foreach (Enum value in enm.Values)
				combo.AppendText (enm[value].Label);
		}

		public override void Dispose ()
		{
			tips.Destroy ();
			base.Dispose ();
		}

		public Enum Value {
			get {
				return enm.Values[combo.Active];
			}
			set {
				int i = Array.IndexOf (enm.Values, value);
				if (i != -1)
					combo.Active = i;
			}
		}

		public event EventHandler ValueChanged;

		void combo_Changed (object o, EventArgs args)
		{
			if (ValueChanged != null)
				ValueChanged (this, EventArgs.Empty);
			EnumValue value = enm[Value];
			if (value != null)
				tips.SetTip (ebox, value.Description, value.Description);
			else
				tips.SetTip (ebox, null, null);
		}
	}
}
