using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Statusbar", "statusbar.png")]
	public class Statusbar : Gtk.Statusbar, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup StatusbarProperties;

		static Statusbar () {
			StatusbarProperties = new PropertyGroup ("Status Bar Properties",
								 typeof (Stetic.Wrapper.Statusbar),
								 "HasResizeGrip");

			groups = new PropertyGroup[] {
				StatusbarProperties,
				Widget.CommonWidgetProperties
			};
		}

		public Statusbar () {}
	}
}
