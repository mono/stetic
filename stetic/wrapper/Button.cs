using Gtk;
using Gdk;
using GLib;
using System;
using System.ComponentModel;

namespace Stetic.Wrapper {

	public class Button : Gtk.Button, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup ButtonProperties;
		public static PropertyGroup ButtonExtraProperties;

		static Button () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Stetic.Wrapper.Button), "StockId"),
				new PropertyDescriptor (typeof (Gtk.Button), "Label"),
//				new PropertyDescriptor (typeof (Stetic.Wrapper.Button), "Icon")
			};				
			ButtonProperties = new PropertyGroup ("Button Properties", props);

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Button), "FocusOnClick"),
				new PropertyDescriptor (typeof (Gtk.Button), "UseUnderline"),
				new PropertyDescriptor (typeof (Gtk.Button), "Relief"),
				new PropertyDescriptor (typeof (Gtk.Button), "Xalign"),
				new PropertyDescriptor (typeof (Gtk.Button), "Yalign"),
				new PropertyDescriptor (typeof (Gtk.Container), "BorderWidth")
			};
			ButtonExtraProperties = new PropertyGroup ("Extra Button Properties", props);

			groups = new PropertyGroup[] {
				ButtonProperties, ButtonExtraProperties,
				Widget.CommonWidgetProperties
			};
		}

		public Button () : base (Gtk.Stock.Ok) {}

		[Editor (typeof (Stetic.Editor.StockItem), typeof (Gtk.Widget))]
		public string StockId {
			get {
				return UseStock ? Label : "";
			}
			set {
				if (value == "" || value == null) {
					UseStock = false;
					EmitSensitivityChanged ("Label", true);
				} else {
					UseStock = true;
					Label = value;
					EmitSensitivityChanged ("Label", false);
				}
			}
		}

		public event SensitivityChangedDelegate SensitivityChanged;
		void EmitSensitivityChanged (string property, bool sensitivity)
		{
			if (SensitivityChanged != null)
				SensitivityChanged (property, sensitivity);
		}
	}
}
