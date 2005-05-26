using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("ScrolledWindow", "scrolledwindow.png", ObjectWrapperType.Container)]
	public class ScrolledWindow : Bin {

		public static new Type WrappedType = typeof (Gtk.ScrolledWindow);

		internal static new void Register (Type type)
		{
			AddItemGroup (type, "ScrolledWindow Properties",
				      "VscrollbarPolicy",
				      "HscrollbarPolicy",
				      "ShadowType",
				      "WindowPlacement",
				      "BorderWidth");
		}

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

		public override IEnumerable GladeChildren {
			get {
				return base.RealChildren;
			}
		}

		public override bool HExpandable { get { return true; } }
		public override bool VExpandable { get { return true; } }

		void AddWithViewport (Gtk.Widget child)
		{
			scrolled.AddWithViewport (child);
			ObjectWrapper.Create (stetic, typeof (Viewport), scrolled.Child);
		}

		protected override void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
		{
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
