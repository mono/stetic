using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Editor {

	public class OptIntRange : Gtk.HBox {

		CheckButton check;
		SpinButton spin;

		public OptIntRange (int min, int max, int initial) : base (false, 6)
		{
			check = new Gtk.CheckButton ();
			check.Show ();
			check.Toggled += check_Toggled;
			PackStart (check, false, false, 0);

			spin = new Gtk.SpinButton ((double)min, (double)max, 1.0);
			spin.Show ();
			spin.ValueChanged += spin_ValueChanged;
			PackStart (spin, true, true, 0);

			Value = initial;
		}

		void check_Toggled (object o, EventArgs args)
		{
			spin.Sensitive = check.Active;
			if (ValueChanged != null)
				ValueChanged (this, EventArgs.Empty);
		}

		void spin_ValueChanged (object o, EventArgs args)
		{
			if (ValueChanged != null)
				ValueChanged (this, EventArgs.Empty);
		}

		public int Value {
			get {
				if (check.Active)
					return (int)spin.Value;
				else
					return -1;
			}
			set {
				if (value == -1) {
					check.Active = false;
					spin.Sensitive = false;
				} else {
					check.Active = true;
					spin.Sensitive = true;
					spin.Value = (double)value;
				}
			}
		}

		public event EventHandler ValueChanged;
	}
}
