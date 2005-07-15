using System;

namespace Stetic.Editor {

	[PropertyEditor ("Text", "Changed")]
	public class String : Translatable {

		Gtk.Entry entry;

		public String (PropertyDescriptor prop, object obj) : base (prop, obj)
		{
			entry = new Gtk.Entry ();
			entry.Show ();
			entry.Changed += EntryChanged;
			Add (entry);
		}

		public string Text {
			get {
				return entry.Text;
			}
			set {
				if (value == null)
					entry.Text = "";
				else
					entry.Text = value;
			}
		}

		void EntryChanged (object obj, EventArgs args)
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		public event EventHandler Changed;
	}
}
