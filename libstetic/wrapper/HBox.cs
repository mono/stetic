using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("HBox", "hbox.png", ObjectWrapperType.Container)]
	public class HBox : Box {
		public HBox (IStetic stetic) : this (stetic, new Gtk.HBox (false, 0)) {}

		public HBox (IStetic stetic, Gtk.HBox hbox) : base (stetic, hbox) {}

		public override bool HExpandable {
			get {
				foreach (WidgetSite site in Sites) {
					if (site.HExpandable)
						return true;
				}
				return false;
			}
		}

		public override bool VExpandable {
			get {
				foreach (WidgetSite site in Sites) {
					if (!site.VExpandable)
						return false;
				}
				return true;
			}
		}
	}
}
