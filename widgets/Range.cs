using Gtk;
using System;

namespace Stetic.Widget {

	public static class Range {
		public static PropertyGroup CommonWidgetProperties;

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
	}
}
