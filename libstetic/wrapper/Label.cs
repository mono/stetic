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

		string mnemonic_widget;

		protected override void GladeImport (string className, string id, Hashtable props)
		{
			mnemonic_widget = GladeUtils.ExtractProperty ("mnemonic_widget", props);
			if (mnemonic_widget != null)
				stetic.GladeImportComplete += SetMnemonicWidget;
			base.GladeImport (className, id, props);
		}

		void SetMnemonicWidget ()
		{
			Gtk.Widget mnem = stetic.LookupWidgetById (mnemonic_widget);
			if (mnem != null)
				((Gtk.Label)Wrapped).MnemonicWidget = mnem;
			stetic.GladeImportComplete -= SetMnemonicWidget;
		}
	}
}
