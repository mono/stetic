using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Label", "label.png", typeof (Gtk.Label), ObjectWrapperType.Widget)]
	public class Label : Misc {

		public static ItemGroup LabelProperties;

		static Label () {
			LabelProperties = new ItemGroup ("Label Properties",
							 typeof (Gtk.Label),
							 "LabelProp",
							 "UseMarkup",
							 "UseUnderline",
							 "Wrap",
							 "MnemonicWidget",
							 "Justify",
							 "Selectable");
			RegisterWrapper (typeof (Stetic.Wrapper.Label),
					 LabelProperties,
					 Misc.MiscProperties,
					 Widget.CommonWidgetProperties);
		}

		public Label (IStetic stetic) : this (stetic, new Gtk.Label (), false) {}
		

		public Label (IStetic stetic, Gtk.Label label, bool initialized) : base (stetic, label, initialized)
		{
			if (!initialized) {
				label.LabelProp = label.Name;
			}
		}
	}
}
