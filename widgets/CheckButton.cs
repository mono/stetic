using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Check Box", "checkbutton.png")]
	public class CheckButton : Gtk.CheckButton, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup CheckButtonProperties;

		static CheckButton () {
			CheckButtonProperties = new PropertyGroup ("Check Box Properties",
								   typeof (Stetic.Wrapper.CheckButton),
								   "Label",
								   "Active",
								   "Inconsistent",
								   "DrawIndicator");

			groups = new PropertyGroup[] {
				CheckButtonProperties, Button.ButtonExtraProperties,
				Widget.CommonWidgetProperties
			};
		}

		public CheckButton () : base ("Check Box") {}
	}
}
