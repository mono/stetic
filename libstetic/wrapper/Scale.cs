using System;

namespace Stetic.Wrapper {

	public abstract class Scale : Widget {

		public static new Type WrappedType = typeof (Gtk.Scale);

		internal static new void Register (Type type)
		{
			ItemGroup props = AddItemGroup (type, "Scale Properties",
							"Adjustment.Lower",
							"Adjustment.Upper",
							"Adjustment.Value",
							"DrawValue",
							"Digits",
							"ValuePos",
							"Inverted");
			props["Digits"].DisabledIf (props["DrawValue"], false);
			props["ValuePos"].DisabledIf (props["DrawValue"], false);

			// The default value in the ParamSpec is wrong, so hide it
			((PropertyDescriptor)props["DrawValue"]).HasDefault = false;
			((PropertyDescriptor)props["ValuePos"]).HasDefault = false;
		}

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
