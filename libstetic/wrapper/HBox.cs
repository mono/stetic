using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("HBox", "hbox.png", ObjectWrapperType.Container)]
	public class HBox : Box {

		public static new Type WrappedType = typeof (Gtk.HBox);

		public static new Gtk.HBox CreateInstance ()
		{
			return new Gtk.HBox (false, 0);
		}

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
