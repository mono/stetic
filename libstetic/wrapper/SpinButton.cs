using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Spin Button", "spinbutton.png", ObjectWrapperType.Widget)]
	public class SpinButton : Stetic.Wrapper.Widget {

		public static PropertyGroup SpinButtonProperties;
		public static PropertyGroup SpinButtonAdjustmentProperties;

		static SpinButton () {
			SpinButtonAdjustmentProperties = new PropertyGroup ("Range Properties",
									    typeof (Gtk.SpinButton),
									    "Adjustment.Lower",
									    "Adjustment.Upper",
									    "Adjustment.PageIncrement",
									    "Adjustment.PageSize",
									    "Adjustment.StepIncrement");
			SpinButtonProperties = new PropertyGroup ("Spin Button Properties",
								  typeof (Gtk.SpinButton),
								  "ClimbRate",
								  "Digits",
								  "Numeric",
								  "SnapToTicks",
								  "UpdatePolicy",
								  "Value",
								  "Wrap");

			groups = new PropertyGroup[] {
				SpinButtonProperties, SpinButtonAdjustmentProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public SpinButton (IStetic stetic) : this (stetic, new Gtk.SpinButton (0.0, 100.0, 1.0)) {}

		public SpinButton (IStetic stetic, Gtk.SpinButton spinbutton) : base (stetic, spinbutton) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
