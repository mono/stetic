using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Window", "window.png", WidgetType.Window)]
	public class Window : Gtk.Window, Stetic.IContainerWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup WindowProperties;
		public static PropertyGroup WindowSizeProperties;
		public static PropertyGroup WindowMiscProperties;

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static Window () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Window), "Title"),
				new PropertyDescriptor (typeof (Gtk.Window), "Icon"),
				new PropertyDescriptor (typeof (Gtk.Window), "Type"),
				new PropertyDescriptor (typeof (Gtk.Window), "TypeHint"),
				new PropertyDescriptor (typeof (Gtk.Window), "WindowPosition"),
				new PropertyDescriptor (typeof (Gtk.Window), "Modal"),
				new PropertyDescriptor (typeof (Gtk.Container), "BorderWidth"),
			};
			WindowProperties = new PropertyGroup ("Window Properties", props);

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Window), "Resizable"),
				new PropertyDescriptor (typeof (Gtk.Window), "AllowGrow"),
				new PropertyDescriptor (typeof (Gtk.Window), "AllowShrink"),
				new PropertyDescriptor (typeof (Gtk.Window), "DefaultWidth"),
				new PropertyDescriptor (typeof (Gtk.Window), "DefaultHeight"),
			};
			WindowSizeProperties = new PropertyGroup ("Window Size Properties", props);

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Window), "AcceptFocus"),
				new PropertyDescriptor (typeof (Gtk.Window), "Decorated"),
				new PropertyDescriptor (typeof (Gtk.Window), "DestroyWithParent"),
				new PropertyDescriptor (typeof (Gtk.Window), "Gravity"),
				new PropertyDescriptor (typeof (Gtk.Window), "Role"),
				new PropertyDescriptor (typeof (Gtk.Window), "SkipPagerHint"),
				new PropertyDescriptor (typeof (Gtk.Window), "SkipTaskbarHint"),
			};				
			WindowMiscProperties = new PropertyGroup ("Miscellaneous Window Properties", props);

			groups = new PropertyGroup[] {
				WindowProperties, WindowSizeProperties, WindowMiscProperties,
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[0];
		}

		public Window () : base ("Window")
		{
			WidgetSite site = new WidgetSite ();
			site.Show ();
			Add (site);
		}

		protected override void OnSizeRequested (ref Requisition req)
		{
			WidgetSite site = (WidgetSite)Child;

			if (site.Occupied)
				req = site.SizeRequest ();
			else
				req.Width = req.Height = 200;
		}

		public bool HExpandable { get { return true; } }
		public bool VExpandable { get { return true; } }

		public event ExpandabilityChangedHandler ExpandabilityChanged;
	}
}
