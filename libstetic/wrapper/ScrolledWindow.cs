using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("ScrolledWindow", "scrolledwindow.png", typeof (Gtk.ScrolledWindow), ObjectWrapperType.Container)]
	public class ScrolledWindow : Bin {

		public static ItemGroup ScrolledWindowProperties;

		static ScrolledWindow () {
			ScrolledWindowProperties = new ItemGroup ("ScrolledWindow Properties",
								  typeof (Gtk.ScrolledWindow),
								  "VscrollbarPolicy",
								  "HscrollbarPolicy",
								  "ShadowType",
								  "WindowPlacement",
								  "BorderWidth");
			RegisterWrapper (typeof (Stetic.Wrapper.ScrolledWindow),
					 ScrolledWindowProperties,
					 Widget.CommonWidgetProperties);
		}

		public ScrolledWindow (IStetic stetic) : this (stetic, new Gtk.ScrolledWindow (), false) {}


		public ScrolledWindow (IStetic stetic, Gtk.ScrolledWindow scrolledwindow, bool initialized) : base (stetic, scrolledwindow, initialized)
		{
			if (!initialized) {
				scrolledwindow.SetPolicy (Gtk.PolicyType.Always, Gtk.PolicyType.Always);
			}
			if (!initialized && scrolledwindow.Child == null) {
				scrolledwindow.AddWithViewport (CreateWidgetSite ());
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
