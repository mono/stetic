using System;

namespace Stetic.Wrapper {

	public abstract class Scale : Widget {

		public static new Type WrappedType = typeof (Gtk.Scale);

		static new void Register (Type type)
		{
			ItemGroup props = AddItemGroup (type, "Scale Properties",
							"Adjustment.Lower",
							"Adjustment.Upper",
							"Adjustment.Value",
							"DrawValue",
							"Digits",
							"ValuePos",
							"Inverted");
			props["Digits"].DependsOn (props["DrawValue"]);
			props["ValuePos"].DependsOn (props["DrawValue"]);
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
