using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("Statusbar", "statusbar.png")]
	public class Statusbar : Gtk.Statusbar, Stetic.IWidgetWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup StatusbarProperties;

		static Statusbar () {
			StatusbarProperties = new PropertyGroup ("Status Bar Properties",
								 typeof (Stetic.Widget.Statusbar),
								 "HasResizeGrip");

			groups = new PropertyGroup[] {
				StatusbarProperties,
				Widget.CommonWidgetProperties
			};
		}

		public Statusbar (IStetic stetic) {}
	}
}
