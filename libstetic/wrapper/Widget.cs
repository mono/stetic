using System;

namespace Stetic.Wrapper {

	public abstract class Widget : Stetic.Wrapper.Object {
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

		protected Widget (IStetic stetic, Gtk.Widget widget) : base (stetic, widget) {}

		public static new Widget Lookup (GLib.Object obj)
		{
			return Object.Lookup (obj) as Stetic.Wrapper.Widget;
		}
	}
}
