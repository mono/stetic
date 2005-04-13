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
				Gtk.ScrolledWindow scrolled = (Gtk.ScrolledWindow)Wrapped;
				scrolled.SetPolicy (Gtk.PolicyType.Always, Gtk.PolicyType.Always);
				if (scrolled.Child == null)
					scrolled.AddWithViewport (CreatePlaceholder ());
			}
		}

		public override bool HExpandable { get { return true; } }
		public override bool VExpandable { get { return true; } }

		protected override void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
		{
			Gtk.ScrolledWindow scwin = (Gtk.ScrolledWindow)Wrapped;

			scwin.Remove (scwin.Child);
			if (newChild.SetScrollAdjustments (null, null))
				scwin.Add (newChild);
			else
				scwin.AddWithViewport (newChild);
		}

		public override Placeholder AddPlaceholder ()
		{
			Gtk.ScrolledWindow scwin = (Gtk.ScrolledWindow)Wrapped;

			Placeholder ph = CreatePlaceholder ();
			scwin.AddWithViewport (ph);
			return ph;
		}
	}
}
