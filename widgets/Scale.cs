using Gtk;
using System;

namespace Stetic.Widget {

	public static class Scale {
		public static PropertyGroup CommonWidgetProperties;

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
		}
	}
}
