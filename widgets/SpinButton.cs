using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("Spin Button", "spinbutton.png")]
	public class SpinButton : Gtk.SpinButton, Stetic.IWidgetWrapper {
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
								  typeof (Stetic.Widget.SpinButton),
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

		public SpinButton (IStetic stetic) : base (0.0, 100.0, 1.0) {}
	}
}
