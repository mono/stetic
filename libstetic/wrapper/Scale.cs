using System;

namespace Stetic.Wrapper {

	public abstract class Scale : Stetic.Wrapper.Widget {
		public static PropertyGroup ScaleProperties;

		static Scale () {
			ScaleProperties = new PropertyGroup ("Scale Properties",
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

			groups = new PropertyGroup[] {
				Scale.ScaleProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		protected Scale (IStetic stetic, Gtk.Scale scale) : base (stetic, scale) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
