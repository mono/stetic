using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("Vertical Scrollbar", "vscrollbar.png")]
	public class VScrollbar : Gtk.VScrollbar, Stetic.IWidgetWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static VScrollbar () {
			groups = new PropertyGroup[] {
				Range.RangeAdjustmentProperties,
				Range.RangeProperties,
				Widget.CommonWidgetProperties
			};
		}

		public VScrollbar (IStetic stetic) : base (new Gtk.Adjustment (0.0, 0.0, 100.0, 1.0, 10.0, 10.0)) {}
	}
}
