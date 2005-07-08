using System;

namespace Stetic.Editor {

	public class OptIntRange : Gtk.HBox {

		Gtk.CheckButton check;
		Gtk.SpinButton spin;

		public OptIntRange (object min, object max) : base (false, 6)
		{
			check = new Gtk.CheckButton ();
			check.Show ();
			check.Toggled += check_Toggled;
			PackStart (check, false, false, 0);

			if (min == null)
				min = Int32.MinValue;
			if (max == null)
				max = Int32.MaxValue;
			spin = new Gtk.SpinButton ((double)(int)min, (double)(int)max, 1.0);
			spin.Show ();
			spin.ValueChanged += spin_ValueChanged;
			PackStart (spin, true, true, 0);
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
