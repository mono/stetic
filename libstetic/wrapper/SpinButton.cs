using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Spin Button", "spinbutton.png", ObjectWrapperType.Widget)]
	public class SpinButton : Stetic.Wrapper.Widget {

		public static ItemGroup SpinButtonProperties;
		public static ItemGroup SpinButtonAdjustmentProperties;

		static SpinButton () {
			SpinButtonAdjustmentProperties = new ItemGroup ("Range Properties",
									typeof (Gtk.SpinButton),
									"Adjustment.Lower",
									"Adjustment.Upper",
									"Adjustment.PageIncrement",
									"Adjustment.PageSize",
									"Adjustment.StepIncrement");
			SpinButtonProperties = new ItemGroup ("Spin Button Properties",
							      typeof (Gtk.SpinButton),
							      "ClimbRate",
							      "Digits",
							      "Numeric",
							      "SnapToTicks",
							      "UpdatePolicy",
							      "Value",
							      "Wrap");
			RegisterWrapper (typeof (Stetic.Wrapper.SpinButton),
					 SpinButtonProperties,
					 SpinButtonAdjustmentProperties,
					 Widget.CommonWidgetProperties);
		}

		public SpinButton (IStetic stetic) : this (stetic, new Gtk.SpinButton (0.0, 100.0, 1.0)) {}
		public SpinButton (IStetic stetic, Gtk.SpinButton spinbutton) : base (stetic, spinbutton) {}
	}
}
