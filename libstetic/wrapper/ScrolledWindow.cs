using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("ScrolledWindow", "scrolledwindow.png", ObjectWrapperType.Container)]
	public class ScrolledWindow : Bin {

		public static new Type WrappedType = typeof (Gtk.ScrolledWindow);

		static new void Register (Type type)
		{
			AddItemGroup (type, "ScrolledWindow Properties",
				      "VscrollbarPolicy",
				      "HscrollbarPolicy",
				      "ShadowType",
				      "WindowPlacement",
				      "BorderWidth");
		}

		protected override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				Gtk.ScrolledWindow scrolled = (Gtk.ScrolledWindow)Wrapped;
				scrolled.SetPolicy (Gtk.PolicyType.Always, Gtk.PolicyType.Always);
				if (scrolled.Child == null)
					scrolled.AddWithViewport (CreateWidgetSite ());
			}
		}

		public override bool HExpandable { get { return true; } }
		public override bool VExpandable { get { return true; } }

		protected override void SiteOccupancyChanged (WidgetSite site)
		{
			Gtk.ScrolledWindow scwin = (Gtk.ScrolledWindow)Wrapped;

			if (scwin.Child == null)
				return;

			if (site.Occupied &&
			    site.Contents.SetScrollAdjustments (null, null)) {
				if (scwin.Child is Gtk.Viewport) {
					((Gtk.Viewport)scwin.Child).Remove (site);
					scwin.Remove (scwin.Child);
					scwin.Add (site);
				}
			} else {
				if (!(scwin.Child is Gtk.Viewport)) {
					scwin.Remove (scwin.Child);
					scwin.AddWithViewport (site);
				}
			}

			base.SiteOccupancyChanged (site);
		}
	}
}
