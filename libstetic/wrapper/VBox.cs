using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("VBox", "vbox.png", ObjectWrapperType.Container)]
	public class VBox : Box {

		public static new Type WrappedType = typeof (Gtk.VBox);

		public override bool HExpandable {
			get {
				foreach (Gtk.Widget w in box.Children) {
					if (!ChildHExpandable (w))
						return false;
				}
				return true;
			}
		}

		public override bool VExpandable {
			get {
				foreach (Gtk.Widget w in box.Children) {
					if (ChildVExpandable (w))
						return true;
				}
				return false;
			}
		}
	}
}
