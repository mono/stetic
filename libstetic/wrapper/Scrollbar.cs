using System;

namespace Stetic.Wrapper {

	public abstract class Scrollbar : Range {
		protected Scrollbar (IStetic stetic, Gtk.Scrollbar scrollbar) : base (stetic, scrollbar) {}
	}
}
