using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("ProgressBar", "progressbar.png", ObjectWrapperType.Widget)]
	public class ProgressBar : Stetic.Wrapper.Widget {

		public static PropertyGroup ProgressBarProperties;

		static ProgressBar () {
			ProgressBarProperties = new PropertyGroup ("Progress Bar Properties",
								   typeof (Gtk.ProgressBar),
								   "Orientation",
								   "Text",
								   "PulseStep");
			groups = new PropertyGroup[] {
				ProgressBarProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public ProgressBar (IStetic stetic) : this (stetic, new Gtk.ProgressBar ()) {}

		public ProgressBar (IStetic stetic, Gtk.ProgressBar progressbar) : base (stetic, progressbar) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
