using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Vertical Separator", "vseparator.png", ObjectWrapperType.Widget)]
	public class VSeparator : Stetic.Wrapper.Widget {

		static VSeparator () {
			groups = new PropertyGroup[] {
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public VSeparator (IStetic stetic) : this (stetic, new Gtk.VSeparator ()) {}

		public VSeparator (IStetic stetic, Gtk.VSeparator vseparator) : base (stetic, vseparator) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
