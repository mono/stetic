using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("ProgressBar", "progressbar.png", ObjectWrapperType.Widget)]
	public class ProgressBar : Stetic.Wrapper.Widget {

		public static ItemGroup ProgressBarProperties;

		static ProgressBar () {
			ProgressBarProperties = new ItemGroup ("Progress Bar Properties",
							       typeof (Gtk.ProgressBar),
							       "Orientation",
							       "Text",
							       "PulseStep");
			groups = new ItemGroup[] {
				ProgressBarProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public ProgressBar (IStetic stetic) : this (stetic, new Gtk.ProgressBar ()) {}

		public ProgressBar (IStetic stetic, Gtk.ProgressBar progressbar) : base (stetic, progressbar) {}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }
	}
}
