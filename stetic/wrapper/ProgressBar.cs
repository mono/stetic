using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	[WidgetWrapper ("ProgressBar", "progressbar.png")]
	public class ProgressBar : Gtk.ProgressBar, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup ProgressBarProperties;

		static ProgressBar () {
			ProgressBarProperties = new PropertyGroup ("Progress Bar Properties",
								   typeof (Stetic.Wrapper.ProgressBar),
								   "Orientation",
								   "Text",
								   "PulseStep");
			groups = new PropertyGroup[] {
				ProgressBarProperties,
				Widget.CommonWidgetProperties
			};
		}

		public ProgressBar () {}
	}
}
