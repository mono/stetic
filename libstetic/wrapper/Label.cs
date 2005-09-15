using System;
using System.Collections;

namespace Stetic.Wrapper {

	public class Label : Widget {

		public Label () {}

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
