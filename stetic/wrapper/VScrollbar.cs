using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class VScrollbar : Gtk.VScrollbar, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static VScrollbar () {
			groups = new PropertyGroup[] {
				Range.RangeAdjustmentProperties,
				Range.RangeProperties,
				Widget.CommonWidgetProperties
			};
		}

		public VScrollbar () : base (new Gtk.Adjustment (0.0, 0.0, 100.0, 1.0, 10.0, 10.0)) {}
	}
}
