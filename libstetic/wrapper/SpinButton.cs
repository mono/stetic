using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Spin Button", "spinbutton.png", ObjectWrapperType.Widget)]
	public class SpinButton : Widget {

		public static new Type WrappedType = typeof (Gtk.SpinButton);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Range Properties",
				      "Adjustment.Lower",
				      "Adjustment.Upper",
				      "Adjustment.PageIncrement",
				      "Adjustment.PageSize",
				      "Adjustment.StepIncrement");
			AddItemGroup (type, "Spin Button Properties",
				      "ClimbRate",
				      "Digits",
				      "Numeric",
				      "SnapToTicks",
				      "UpdatePolicy",
				      "Value",
				      "Wrap");
		}

		public static new Gtk.SpinButton CreateInstance ()
		{
			return new Gtk.SpinButton (0.0, 100.0, 1.0);
		}
	}
}
