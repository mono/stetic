using System;
using System.Collections;

namespace Stetic.Wrapper {

	public class ScrolledWindow : Container {

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				scrolled.SetPolicy (Gtk.PolicyType.Always, Gtk.PolicyType.Always);
				if (scrolled.Child == null)
					AddPlaceholder ();
			}
		}

		public Gtk.ScrolledWindow scrolled {
			get {
				return (Gtk.ScrolledWindow)Wrapped;
			}
		}

		public override IEnumerable RealChildren {
			get {
				if (scrolled.Child is Gtk.Viewport)
					return ((Gtk.Viewport)scrolled.Child).Children;
				else
					return base.RealChildren;
			}
		}

		void AddWithViewport (Gtk.Widget child)
		{
			Gtk.Viewport viewport = new Gtk.Viewport (scrolled.Hadjustment, scrolled.Vadjustment);
			ObjectWrapper.Create (proj, viewport);
			viewport.ShadowType = Gtk.ShadowType.None;
			viewport.Add (child);
			viewport.Show ();
			scrolled.Add (viewport);
		}

		public override void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
		{
			if (scrolled.Child is Gtk.Viewport)
				((Gtk.Viewport)scrolled.Child).Remove (oldChild);
			scrolled.Remove (scrolled.Child);
			if (newChild.SetScrollAdjustments (null, null))
				scrolled.Add (newChild);
			else
				AddWithViewport (newChild);
		}

		public override Placeholder AddPlaceholder ()
		{
			Placeholder ph = CreatePlaceholder ();
			AddWithViewport (ph);
			return ph;
		}
	}
}
