using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Statusbar", "statusbar.png", ObjectWrapperType.Widget)]
	public class Statusbar : Stetic.Wrapper.Widget {

		public static ItemGroup StatusbarProperties;

		static Statusbar () {
			StatusbarProperties = new ItemGroup ("Status Bar Properties",
							     typeof (Gtk.Statusbar),
							     "HasResizeGrip");
			RegisterItems (typeof (Stetic.Wrapper.Statusbar),
				       StatusbarProperties,
				       Widget.CommonWidgetProperties);
		}

		public Statusbar (IStetic stetic) : this (stetic, new Gtk.Statusbar ()) {}
		public Statusbar (IStetic stetic, Gtk.Statusbar statusbar) : base (stetic, statusbar) {}
	}
}
