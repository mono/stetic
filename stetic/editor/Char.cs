using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Editor {

	public class Char : Gtk.Entry {

		char initial;

		public Char (char initial)
		{
			MaxLength = 1;

			this.initial = initial;
			Text = initial.ToString ();
		}

		public char Value {
			get {
				if (Text.Length == 0)
					return initial;
				else
					return Text[0];
			}
			set {
				Text = value.ToString ();
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
