using System;

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
	}
}
