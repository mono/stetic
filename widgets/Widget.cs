using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public static class Widget {
		public static PropertyGroup CommonWidgetProperties;

		static Widget () {
			CommonWidgetProperties = new PropertyGroup ("Common Widget Properties",
								    typeof (Gtk.Widget),
								    "WidthRequest",
								    "HeightRequest",
								    "Visible",
								    "Sensitive",
								    "CanDefault",
								    "HasDefault",
								    "CanFocus",
								    "HasFocus",
								    "Events",
								    "ExtensionEvents");
		}
	}
}
