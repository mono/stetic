using System;

namespace Stetic.Wrapper {

	public class Paned : Container {

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				paned.Pack1 (CreatePlaceholder (), true, false);
				paned.Pack2 (CreatePlaceholder (), true, false);
			}
		}

		protected Gtk.Paned paned {
			get {
				return (Gtk.Paned)Wrapped;
			}
		}

		public override void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
		{
			if (oldChild == paned.Child1) {
				paned.Remove (oldChild);
				paned.Add1 (newChild);
			} else if (oldChild == paned.Child2) {
				paned.Remove (oldChild);
				paned.Add2 (newChild);
			}
		}
	}
}
