using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Frame", "frame.png", WidgetType.Container)]
	public class Frame : Gtk.Frame, Stetic.IContainerWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup FrameProperties;

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static Frame () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Frame), "Shadow"),
				new PropertyDescriptor (typeof (Gtk.Frame), "ShadowType"),
				new PropertyDescriptor (typeof (Gtk.Frame), "Label"),
				new PropertyDescriptor (typeof (Gtk.Frame), "LabelXalign"),
				new PropertyDescriptor (typeof (Gtk.Frame), "LabelYalign"),
				new PropertyDescriptor (typeof (Gtk.Container), "BorderWidth"),
			};				
			FrameProperties = new PropertyGroup ("Frame Properties", props);

			groups = new PropertyGroup[] {
				FrameProperties,
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[0];
		}

		public Frame () : base ("Frame")
		{
			WidgetSite site = new WidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			Add (site);
		}

		public bool HExpandable { get { return ((WidgetSite)Child).HExpandable; } }
		public bool VExpandable { get { return ((WidgetSite)Child).VExpandable; } }

		public event ExpandabilityChangedHandler ExpandabilityChanged;

		private void SiteOccupancyChanged (WidgetSite site)
		{
			if (ExpandabilityChanged != null)
				ExpandabilityChanged (this);
		}
	}
}
