using Gtk;
using System;

namespace Stetic.Widget {

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
