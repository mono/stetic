using System;

namespace Stetic.Wrapper {

	public abstract class Scale : Stetic.Wrapper.Widget {
		public static ItemGroup ScaleProperties;

		static Scale () {
			ScaleProperties = new ItemGroup ("Scale Properties",
							 typeof (Gtk.Scale),
							 "Adjustment.Lower",
							 "Adjustment.Upper",
							 "Adjustment.Value",
							 "DrawValue",
							 "Digits",
							 "ValuePos",
							 "Inverted");
			ScaleProperties["Digits"].DependsOn (ScaleProperties["DrawValue"]);
			ScaleProperties["ValuePos"].DependsOn (ScaleProperties["DrawValue"]);

			RegisterWrapper (typeof (Stetic.Wrapper.Scale),
					 Scale.ScaleProperties,
					 Widget.CommonWidgetProperties);
		}


		protected Scale (IStetic stetic, Gtk.Scale scale, bool initialized) : base (stetic, scale, initialized)
		{
			scale.Adjustment.AddNotification (AdjustmentNotifyHandler);
		}

		void AdjustmentNotifyHandler (object obj, GLib.NotifyArgs args)
		{
			EmitNotify (args.Property);
		}
	}
}