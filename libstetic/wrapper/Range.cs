using System;

namespace Stetic.Wrapper {

	public abstract class Range : Stetic.Wrapper.Widget {
		public static PropertyGroup RangeProperties;
		public static PropertyGroup RangeAdjustmentProperties;

		static Range () {
			RangeProperties = new PropertyGroup ("Range Properties",
							     typeof (Gtk.Range),
							     "UpdatePolicy",
							     "Inverted");
			RangeAdjustmentProperties = new PropertyGroup ("Adjustment Properties",
								       typeof (Gtk.Range),
								       "Adjustment.Lower",
								       "Adjustment.Upper",
								       "Adjustment.PageIncrement",
								       "Adjustment.PageSize",
								       "Adjustment.StepIncrement",
								       "Adjustment.Value");
		}

		protected Range (IStetic stetic, Gtk.Range range) : base (stetic, range) {}
	}
}
