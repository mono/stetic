using System;

namespace Stetic.Editor {

	[PropertyEditor ("Text", "Changed")]
	public class String : Gtk.Entry {

		public new string Text {
			get {
				return base.Text;
			}
			set {
				if (value == null)
					base.Text = "";
				else
					base.Text = value;
			}
		}
	}
}
