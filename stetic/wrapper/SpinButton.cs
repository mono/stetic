using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Spin Button", "spinbutton.png")]
	public class SpinButton : Gtk.SpinButton, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup SpinButtonProperties;
		public static PropertyGroup SpinButtonAdjustmentProperties;

		static SpinButton () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.SpinButton), "Adjustment.Lower"),
				new PropertyDescriptor (typeof (Gtk.SpinButton), "Adjustment.Upper"),
				new PropertyDescriptor (typeof (Gtk.SpinButton), "Adjustment.PageIncrement"),
				new PropertyDescriptor (typeof (Gtk.SpinButton), "Adjustment.PageSize"),
				new PropertyDescriptor (typeof (Gtk.SpinButton), "Adjustment.StepIncrement"),
			};
			SpinButtonAdjustmentProperties = new PropertyGroup ("Range Properties", props);

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.SpinButton), "ClimbRate"),
				new PropertyDescriptor (typeof (Gtk.SpinButton), "Digits"),
				new PropertyDescriptor (typeof (Gtk.SpinButton), "Numeric"),
				new PropertyDescriptor (typeof (Gtk.SpinButton), "SnapToTicks"),
				new PropertyDescriptor (typeof (Gtk.SpinButton), "UpdatePolicy"),
				new PropertyDescriptor (typeof (Gtk.SpinButton), "Value"),
				new PropertyDescriptor (typeof (Gtk.SpinButton), "Wrap"),
			};				
			SpinButtonProperties = new PropertyGroup ("Spin Button Properties", props);

			groups = new PropertyGroup[] {
				SpinButtonProperties, SpinButtonAdjustmentProperties,
				Widget.CommonWidgetProperties
			};
		}

		public SpinButton () : base (0.0, 100.0, 1.0) {}
	}
}
