using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Statusbar", "statusbar.png", typeof (Gtk.Statusbar), ObjectWrapperType.Widget)]
	public class Statusbar : Stetic.Wrapper.Widget {

		public static ItemGroup StatusbarProperties;

		static Statusbar () {
			StatusbarProperties = new ItemGroup ("Status Bar Properties",
							     typeof (Gtk.Statusbar),
							     "HasResizeGrip");
			RegisterWrapper (typeof (Stetic.Wrapper.Statusbar),
					 StatusbarProperties,
					 Widget.CommonWidgetProperties);
		}

		public Statusbar (IStetic stetic) : this (stetic, new Gtk.Statusbar (), false) {}
		public Statusbar (IStetic stetic, Gtk.Statusbar statusbar, bool initialized) : base (stetic, statusbar, initialized) {}
	}
}
