using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Label", "label.png", ObjectWrapperType.Widget)]
	public class Label : Misc {

		public static PropertyGroup LabelProperties;

		static Label () {
			LabelProperties = new PropertyGroup ("Label Properties",
							     typeof (Gtk.Label),
							     "LabelProp",
							     "UseMarkup",
							     "UseUnderline",
							     "Wrap",
							     "MnemonicWidget",
							     "Justify",
							     "Selectable");

			groups = new PropertyGroup[] {
				LabelProperties, Misc.MiscProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public Label (IStetic stetic) : this (stetic, new Gtk.Label ("Label:")) {}

		public Label (IStetic stetic, Gtk.Label label) : base (stetic, label) {}

		public Label (IStetic stetic, string label) : base (stetic, new Gtk.Label (label)) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
