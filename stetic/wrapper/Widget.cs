using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public static class Widget {
		public static PropertyGroup CommonWidgetProperties;

		static Widget () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Widget), "WidthRequest"),
				new PropertyDescriptor (typeof (Gtk.Widget), "HeightRequest"),
				new PropertyDescriptor (typeof (Gtk.Widget), "Visible"),
				new PropertyDescriptor (typeof (Gtk.Widget), "Sensitive"),
				new PropertyDescriptor (typeof (Gtk.Widget), "CanDefault"),
				new PropertyDescriptor (typeof (Gtk.Widget), "HasDefault"),
				new PropertyDescriptor (typeof (Gtk.Widget), "CanFocus"),
				new PropertyDescriptor (typeof (Gtk.Widget), "HasFocus"),
				new PropertyDescriptor (typeof (Gtk.Widget), "Events"),
				new PropertyDescriptor (typeof (Gtk.Widget), "ExtensionEvents"),
			};
			CommonWidgetProperties = new PropertyGroup ("Common Widget Properties", props);
		}
	}
}
