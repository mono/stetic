using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public static class Range {
		public static PropertyGroup CommonWidgetProperties;

		public static PropertyGroup RangeProperties;
		public static PropertyGroup RangeAdjustmentProperties;

		static Range () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Range), "UpdatePolicy"),
				new PropertyDescriptor (typeof (Gtk.Range), "Inverted"),
			};				
			RangeProperties = new PropertyGroup ("Range Properties", props);

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Range), "Adjustment.Lower"),
				new PropertyDescriptor (typeof (Gtk.Range), "Adjustment.Upper"),
				new PropertyDescriptor (typeof (Gtk.Range), "Adjustment.PageIncrement"),
				new PropertyDescriptor (typeof (Gtk.Range), "Adjustment.PageSize"),
				new PropertyDescriptor (typeof (Gtk.Range), "Adjustment.StepIncrement"),
				new PropertyDescriptor (typeof (Gtk.Range), "Adjustment.Value")
			};
			RangeAdjustmentProperties = new PropertyGroup ("Adjustment Properties", props);
		}
	}
}
