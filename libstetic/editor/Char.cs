using System;

namespace Stetic.Editor {

	public class Char : Gtk.Entry {

		public Char ()
		{
			MaxLength = 1;
		}

		char last;

		public char Value {
			get {
				if (Text.Length == 0)
					return last;
				else
					return Text[0];
			}
			set {
				Text = value.ToString ();
				last = value;
			}
		}

		protected override void OnChanged ()
		{
			if (ValueChanged != null)
				ValueChanged (this, EventArgs.Empty);
		}

		public event EventHandler ValueChanged;
	}
}
