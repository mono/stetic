using System;
using System.Collections;

namespace Stetic.Wrapper {

	public class TextView : Container {

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			((Gtk.TextView)Wrapped).Buffer.Changed += Buffer_Changed;
		}

		public string Text {
			get {
				return ((Gtk.TextView)Wrapped).Buffer.Text;
			}
			set {
				((Gtk.TextView)Wrapped).Buffer.Text = value;
			}
		}

		public void Buffer_Changed (object obj, EventArgs args)
		{
			EmitNotify ("Text");
		}
	}
}
