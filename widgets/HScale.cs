using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("Horizontal Scale", "hscale.png")]
	public class HScale : Gtk.HScale, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static HScale () {
			groups = new PropertyGroup[] {
				Scale.ScaleProperties,
				Widget.CommonWidgetProperties
			};
		}

		public HScale (IStetic stetic) : base (0.0, 100.0, 1.0) {}
	}
}
