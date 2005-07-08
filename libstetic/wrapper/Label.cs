using System;
using System.Collections;

namespace Stetic.Wrapper {

	public class Label : Widget {

		public Label () {}

		public Label (string text)
		{
			Wrap (new Gtk.Label (text), true);
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				Gtk.Label label = (Gtk.Label)Wrapped;
				label.LabelProp = label.Name;
			}
		}

		string mnem;
		public string MnemonicWidget {
			get {
				return mnem;
			}
			set {
				mnem = value;
				((Gtk.Label)Wrapped).MnemonicWidget = stetic.LookupWidgetById (mnem);
			}
		}
	}
}
