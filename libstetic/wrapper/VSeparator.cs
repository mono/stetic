using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Vertical Separator", "vseparator.png", ObjectWrapperType.Widget)]
	public class VSeparator : Stetic.Wrapper.Widget {

		static VSeparator () {
			groups = new ItemGroup[] {
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public VSeparator (IStetic stetic) : this (stetic, new Gtk.VSeparator ()) {}

		public VSeparator (IStetic stetic, Gtk.VSeparator vseparator) : base (stetic, vseparator) {}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }
	}
}
