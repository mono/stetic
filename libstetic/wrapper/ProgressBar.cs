using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("ProgressBar", "progressbar.png", typeof (Gtk.ProgressBar), ObjectWrapperType.Widget)]
	public class ProgressBar : Stetic.Wrapper.Widget {

		public static ItemGroup ProgressBarProperties;

		static ProgressBar () {
			ProgressBarProperties = new ItemGroup ("Progress Bar Properties",
							       typeof (Gtk.ProgressBar),
							       "Orientation",
							       "Text",
							       "PulseStep");
			RegisterWrapper (typeof (Stetic.Wrapper.ProgressBar),
					 ProgressBarProperties,
					 Widget.CommonWidgetProperties);
		}

		public ProgressBar (IStetic stetic) : this (stetic, new Gtk.ProgressBar (), false) {}
		public ProgressBar (IStetic stetic, Gtk.ProgressBar progressbar, bool initialized) : base (stetic, progressbar, initialized) {}
	}
}
