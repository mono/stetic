using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("Horizontal Scale", "hscale.png")]
	public class VScale : Gtk.VScale, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static VScale () {
			groups = new PropertyGroup[] {
				Scale.ScaleProperties,
				Widget.CommonWidgetProperties
			};
		}

		public VScale (IStetic stetic) : base (0.0, 100.0, 1.0) {}
	}
}
