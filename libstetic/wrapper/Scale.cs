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

			groups = new ItemGroup[] {
				Scale.ScaleProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		protected Scale (IStetic stetic, Gtk.Scale scale) : base (stetic, scale) {}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }
	}
}
