using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("VBox", "vbox.png", typeof (Gtk.VBox), ObjectWrapperType.Container)]
	public class VBox : Box {

		public VBox (IStetic stetic) : this (stetic, new Gtk.VBox (false, 0), false) {}
		public VBox (IStetic stetic, Gtk.VBox vbox, bool initialized) : base (stetic, vbox, initialized) {}

		public override bool HExpandable {
			get {
				foreach (WidgetSite site in Sites) {
					if (!site.HExpandable)
						return false;
				}
				return true;
			}
		}

		public override bool VExpandable {
			get {
				foreach (WidgetSite site in Sites) {
					if (site.VExpandable)
						return true;
				}
				return false;
			}
		}
	}
}
