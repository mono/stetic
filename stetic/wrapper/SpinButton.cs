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
			SpinButtonAdjustmentProperties = new PropertyGroup ("Range Properties",
									    typeof (Gtk.Range),
									    "Adjustment.Lower",
									    "Adjustment.Upper",
									    "Adjustment.PageIncrement",
									    "Adjustment.PageSize",
									    "Adjustment.StepIncrement");
			SpinButtonProperties = new PropertyGroup ("Spin Button Properties",
								  typeof (Stetic.Wrapper.SpinButton),
								  "ClimbRate",
								  "Digits",
								  "Numeric",
								  "SnapToTicks",
								  "UpdatePolicy",
								  "Value",
								  "Wrap");

			groups = new PropertyGroup[] {
				SpinButtonProperties, SpinButtonAdjustmentProperties,
				Widget.CommonWidgetProperties
			};
		}

		public SpinButton () : base (0.0, 100.0, 1.0) {}
	}
}
