using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Dialog Box", "dialog.png", WidgetType.Window)]
	public class Dialog : Gtk.Dialog, Stetic.IContainerWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup DialogProperties;
		public static PropertyGroup DialogMiscProperties;

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static Dialog () {
			PropertyDescriptor[] props;

			DialogProperties = new PropertyGroup ("Dialog Properties", Window.WindowProperties.Properties);

			props = new PropertyDescriptor[Window.WindowMiscProperties.Properties.Length + 1];
			props[0] = new PropertyDescriptor (typeof (Gtk.Dialog), "HasSeparator");
			Window.WindowMiscProperties.Properties.CopyTo (props, 1);
			DialogMiscProperties = new PropertyGroup ("Miscellaneous Dialog Properties", props);

			groups = new PropertyGroup[] {
				DialogProperties, DialogMiscProperties,
				Window.WindowSizeProperties,
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[0];
		}

		public Dialog () : base ()
		{
			WidgetSite site = new WidgetSite (200, 200);
			site.Show ();
			VBox.Add (site);
		}

		public bool HExpandable { get { return true; } }
		public bool VExpandable { get { return true; } }

		public event ExpandabilityChangedHandler ExpandabilityChanged;
	}
}
