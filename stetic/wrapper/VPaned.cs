using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class VPaned : Gtk.VPaned, Stetic.IContainerWrapper, Stetic.IDesignTimeContainer {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static VPaned () {
			groups = new PropertyGroup[] {
				Paned.PanedProperties,
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[] {
				Paned.PanedChildProperties
			};
		}

		public VPaned ()
		{
			WidgetSite site;

			site = new WidgetSite ();
			site.OccupancyChanged += ChildOccupancyChanged;
			Pack1 (site, true, false);

			site = new WidgetSite ();
			site.OccupancyChanged += ChildOccupancyChanged;
			Pack2 (site, true, false);
		}

		public bool HExpandable {
			get {
				foreach (Gtk.Widget w in Children) {
					WidgetSite site = (WidgetSite)w;

					if (!site.HExpandable)
						return false;
				}
				return true;
			}
		}
		public bool VExpandable { get { return true; } }

		public event OccupancyChangedHandler OccupancyChanged;

		private void ChildOccupancyChanged (IDesignTimeContainer container)
		{
			if (OccupancyChanged != null)
				OccupancyChanged (this);
		}
	}
}
