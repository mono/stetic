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
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Label), "LabelProp"),
				new PropertyDescriptor (typeof (Gtk.Label), "UseMarkup"),
				new PropertyDescriptor (typeof (Gtk.Label), "UseUnderline"),
				new PropertyDescriptor (typeof (Gtk.Label), "Wrap"),
				new PropertyDescriptor (typeof (Gtk.Label), "MnemonicWidget"),
				new PropertyDescriptor (typeof (Gtk.Label), "Justify"),
				new PropertyDescriptor (typeof (Gtk.Label), "Selectable"),
			};				
			LabelProperties = new PropertyGroup ("Label Properties", props);

			groups = new PropertyGroup[] {
				LabelProperties, Misc.MiscProperties,
				Widget.CommonWidgetProperties
			};
		}

		public Label () : base ("Label:") {}

		public Label (string label) : base (label) {}
	}
}
