using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("Check Box", "checkbutton.png")]
	public class CheckButton : Gtk.CheckButton, Stetic.IWidgetWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup CheckButtonProperties;

		static CheckButton () {
			CheckButtonProperties = new PropertyGroup ("Check Box Properties",
								   typeof (Stetic.Widget.CheckButton),
								   "Label",
								   "Active",
								   "Inconsistent",
								   "DrawIndicator");

			groups = new PropertyGroup[] {
				CheckButtonProperties, Button.ButtonExtraProperties,
				Widget.CommonWidgetProperties
			};
		}

		public CheckButton (IStetic stetic) : base ("Check Box") {}
	}
}
