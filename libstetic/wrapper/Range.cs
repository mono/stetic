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
		}

		protected Range (IStetic stetic, Gtk.Range range) : base (stetic, range)
		{
			range.Adjustment.AddNotification (AdjustmentNotifyHandler);
		}

		void AdjustmentNotifyHandler (object obj, GLib.NotifyArgs args)
		{
			EmitNotify (args.Property);
		}
	}
}
