using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Label", "label.png")]
	public class Label : Gtk.Label, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup LabelProperties;

		static Label () {
			LabelProperties = new PropertyGroup ("Label Properties",
							     typeof (Stetic.Wrapper.Label),
							     "LabelProp",
							     "UseMarkup",
							     "UseUnderline",
							     "Wrap",
							     "MnemonicWidget",
							     "Justify",
							     "Selectable");

			groups = new PropertyGroup[] {
				LabelProperties, Misc.MiscProperties,
				Widget.CommonWidgetProperties
			};
		}

		public Label () : base ("Label:") {}

		public Label (string label) : base (label) {}
	}
}
