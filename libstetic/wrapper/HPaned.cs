using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("HPaned", "hpaned.png", ObjectWrapperType.Container)]
	public class HPaned : Paned {

		public static new Type WrappedType = typeof (Gtk.HPaned);

		public override bool HExpandable { get { return true; } }
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
