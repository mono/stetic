using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Horizontal Separator", "hseparator.png", ObjectWrapperType.Widget)]
	public class HSeparator : Stetic.Wrapper.Widget {

		static HSeparator () {
			groups = new PropertyGroup[] {
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public HSeparator (IStetic stetic) : this (stetic, new Gtk.HSeparator ()) {}

		public HSeparator (IStetic stetic, Gtk.HSeparator hseparator) : base (stetic, hseparator) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
