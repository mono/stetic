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

		protected override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				Gtk.Label label = (Gtk.Label)Wrapped;
				label.LabelProp = label.Name;
			}
		}

		string mnemonic_widget;

		protected override void GladeImport (string className, string id, ArrayList propNames, ArrayList propVals)
		{
			mnemonic_widget = GladeUtils.ExtractProperty ("mnemonic_widget", propNames, propVals);
			if (mnemonic_widget != null)
				stetic.GladeImportComplete += SetMnemonicWidget;
			base.GladeImport (className, id, propNames, propVals);
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
