using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("Horizontal Scrollbar", "hscrollbar.png")]
	public class HScrollbar : Gtk.HScrollbar, Stetic.IWidgetWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static HScrollbar () {
			groups = new PropertyGroup[] {
				Range.RangeAdjustmentProperties,
				Range.RangeProperties,
				Widget.CommonWidgetProperties
			};
		}

		public HScrollbar (IStetic stetic) : base (new Gtk.Adjustment (0.0, 0.0, 100.0, 1.0, 10.0, 10.0)) {}
	}
}
