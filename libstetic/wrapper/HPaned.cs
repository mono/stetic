using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("HPaned", "hpaned.png", typeof (Gtk.HPaned), ObjectWrapperType.Container)]
	public class HPaned : Paned {

		public HPaned (IStetic stetic) : this (stetic, new Gtk.HPaned (), false) {}
		public HPaned (IStetic stetic, Gtk.HPaned hpaned, bool initialized) : base (stetic, hpaned, initialized) {}

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
