using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("ScrolledWindow", "scrolledwindow.png", ObjectWrapperType.Container)]
	public class ScrolledWindow : Stetic.Wrapper.Container {

		public static ItemGroup ScrolledWindowProperties;

		static ScrolledWindow () {
			ScrolledWindowProperties = new ItemGroup ("ScrolledWindow Properties",
								  typeof (Gtk.ScrolledWindow),
								  "VscrollbarPolicy",
								  "HscrollbarPolicy",
								  "ShadowType",
								  "WindowPlacement",
								  "BorderWidth");
			groups = new ItemGroup[] {
				ScrolledWindowProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new ItemGroup[0];
		}

		public ScrolledWindow (IStetic stetic) : this (stetic, new Gtk.ScrolledWindow ()) {}

		public ScrolledWindow (IStetic stetic, Gtk.ScrolledWindow scrolledwindow) : base (stetic, scrolledwindow)
		{
			scrolledwindow.SetPolicy (Gtk.PolicyType.Always, Gtk.PolicyType.Always);
			scrolledwindow.AddWithViewport (CreateWidgetSite ());
		}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }

		static ItemGroup[] childgroups;
		public override ItemGroup[] ChildItemGroups { get { return childgroups; } }

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
