using System;

namespace Stetic.Wrapper {

	public abstract class Range : Widget {

		public static new Type WrappedType = typeof (Gtk.Range);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Range Properties",
				      "UpdatePolicy",
				      "Inverted");
			AddItemGroup (type, "Adjustment Properties",
				      "Adjustment.Lower",
				      "Adjustment.Upper",
				      "Adjustment.PageIncrement",
				      "Adjustment.PageSize",
				      "Adjustment.StepIncrement",
				      "Adjustment.Value");
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			((Gtk.Range)Wrapped).Adjustment.AddNotification (AdjustmentNotifyHandler);
		}

		void AdjustmentNotifyHandler (object obj, GLib.NotifyArgs args)
		{
			EmitNotify (args.Property);
		}
	}
}
