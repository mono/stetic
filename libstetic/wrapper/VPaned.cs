using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("VPaned", "vpaned.png", ObjectWrapperType.Container)]
	public class VPaned : Paned {

		public VPaned (IStetic stetic) : this (stetic, new Gtk.VPaned ()) {}
		public VPaned (IStetic stetic, Gtk.VPaned vpaned) : base (stetic, vpaned) {}

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
