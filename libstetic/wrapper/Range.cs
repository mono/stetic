using System;

namespace Stetic.Wrapper {

	public abstract class Range : Stetic.Wrapper.Widget {
		public static ItemGroup RangeProperties;
		public static ItemGroup RangeAdjustmentProperties;

		static Range () {
			RangeProperties = new ItemGroup ("Range Properties",
							 typeof (Gtk.Range),
							 "UpdatePolicy",
							 "Inverted");
			RangeAdjustmentProperties = new ItemGroup ("Adjustment Properties",
								   typeof (Gtk.Range),
								   "Adjustment.Lower",
								   "Adjustment.Upper",
								   "Adjustment.PageIncrement",
								   "Adjustment.PageSize",
								   "Adjustment.StepIncrement",
								   "Adjustment.Value");

			RegisterWrapper (typeof (Stetic.Wrapper.Range),
					 Range.RangeAdjustmentProperties,
					 Range.RangeProperties,
					 Widget.CommonWidgetProperties);
		}

		
		protected Range (IStetic stetic, Gtk.Range range, bool initialized) : base (stetic, range, initialized)
		{
			range.Adjustment.AddNotification (AdjustmentNotifyHandler);
		}

		void AdjustmentNotifyHandler (object obj, GLib.NotifyArgs args)
		{
			EmitNotify (args.Property);
		}
	}
}
