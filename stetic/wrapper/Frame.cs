using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class Frame : Gtk.Frame, Stetic.IObjectWrapper, Stetic.IDesignTimeContainer {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup FrameProperties;

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
		}

		public Frame (string label) : base (label)
		{
			WidgetSite site = new WidgetSite ();
			site.OccupancyChanged += ChildOccupancyChanged;
			Add (site);
		}

		public bool HExpandable { get { return ((WidgetSite)Child).HExpandable; } }
		public bool VExpandable { get { return ((WidgetSite)Child).VExpandable; } }

		public event OccupancyChangedHandler OccupancyChanged;

		private void ChildOccupancyChanged (IDesignTimeContainer container)
		{
			if (OccupancyChanged != null)
				OccupancyChanged (this);
		}
	}
}
