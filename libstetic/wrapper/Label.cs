using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Label", "label.png", ObjectWrapperType.Widget)]
	public class Label : Misc {

		public static new Type WrappedType = typeof (Gtk.Label);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Label Properties",
				      "LabelProp",
				      "UseMarkup",
				      "UseUnderline",
				      "Wrap",
				      "MnemonicWidget",
				      "Justify",
				      "Selectable");
		}

		public Label () {}

		public Label (string text)
		{
			Wrap (new Gtk.Label (text), false);
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				Gtk.Label label = (Gtk.Label)Wrapped;
				label.LabelProp = label.Name;
			}
		}

		[GladeProperty (GladeProperty.LateImport, Proxy = "GladeMnemonicWidget")]
		public Gtk.Widget MnemonicWidget {
			get {
				return ((Gtk.Label)Wrapped).MnemonicWidget;
			}
			set {
				((Gtk.Label)Wrapped).MnemonicWidget = value;
			}
		}

		private string GladeMnemonicWidget {
			get {
				Gtk.Widget mnem = MnemonicWidget;
				if (mnem == null)
					return null;
				else
					return mnem.Name;
			}
			set {
				Gtk.Widget mnem = stetic.LookupWidgetById (value);
				MnemonicWidget = mnem;
			}
		}
	}
}
