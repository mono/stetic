using System;
using System.Collections;

namespace Stetic.Wrapper {

	public class Label : Widget {

		public Label () {}

		public Label (string text)
		{
			Wrap (new Gtk.Label (text), true);
		}

		string mnem;
		public string MnemonicWidget {
			get {
				return mnem;
			}
			set {
				mnem = value;
				((Gtk.Label)Wrapped).MnemonicWidget = proj.LookupWidgetById (mnem);
			}
		}
	}
}
