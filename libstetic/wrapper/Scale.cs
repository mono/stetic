using System;

namespace Stetic.Wrapper {

	public abstract class Scale : Widget {

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			((Gtk.Scale)Wrapped).Adjustment.AddNotification (AdjustmentNotifyHandler);
		}

		void AdjustmentNotifyHandler (object obj, GLib.NotifyArgs args)
		{
			EmitNotify (args.Property);
		}
	}
}
