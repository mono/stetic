using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("VPaned", "vpaned.png", ObjectWrapperType.Container)]
	public class VPaned : Paned {

		public static new Type WrappedType = typeof (Gtk.VPaned);

		public override bool HExpandable {
			get {
				foreach (WidgetSite site in Sites) {
					if (!site.HExpandable)
						return false;
				}
				return true;
			}
		}
		public override bool VExpandable { get { return true; } }
	}
}
