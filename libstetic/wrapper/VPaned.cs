using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("VPaned", "vpaned.png", ObjectWrapperType.Container)]
	public class VPaned : Paned {

		public static new Type WrappedType = typeof (Gtk.VPaned);

		public override bool HExpandable {
			get {
				foreach (Gtk.Widget w in paned) {
					if (!ChildHExpandable (w))
						return false;
				}
				return true;
			}
		}
		public override bool VExpandable { get { return true; } }
	}
}
