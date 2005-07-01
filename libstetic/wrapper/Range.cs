using System;

namespace Stetic.Wrapper {

	public abstract class Range : Widget {

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
