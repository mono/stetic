using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Horizontal Scrollbar", "hscrollbar.png")]
	public class HScrollbar : Gtk.HScrollbar, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static HScrollbar () {
			groups = new PropertyGroup[] {
				Range.RangeAdjustmentProperties,
				Range.RangeProperties,
				Widget.CommonWidgetProperties
			};
		}

		public HScrollbar () : base (new Gtk.Adjustment (0.0, 0.0, 100.0, 1.0, 10.0, 10.0)) {}
	}
}
