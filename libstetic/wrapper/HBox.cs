using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("HBox", "hbox.png", ObjectWrapperType.Container)]
	public class HBox : Box {

		public static new Type WrappedType = typeof (Gtk.HBox);

		public override bool HExpandable {
			get {
				foreach (Gtk.Widget w in box) {
					if (ChildHExpandable (w))
						return true;
				}
				return false;
			}
		}

		public override bool VExpandable {
			get {
				foreach (Gtk.Widget w in box) {
					if (!ChildVExpandable (w))
						return false;
				}
				return true;
			}
		}
	}
}
