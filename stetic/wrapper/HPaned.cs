using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class HPaned : Gtk.HPaned, Stetic.IContainerWrapper, Stetic.IDesignTimeContainer {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static HPaned () {
			groups = new PropertyGroup[] {
				Paned.PanedProperties,
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[] {
				Paned.PanedChildProperties
			};
		}

		public HPaned ()
		{
			WidgetSite site;

			site = new WidgetSite ();
			site.OccupancyChanged += ChildOccupancyChanged;
			Pack1 (site, true, false);

			site = new WidgetSite ();
			site.OccupancyChanged += ChildOccupancyChanged;
			Pack2 (site, true, false);
		}

		public bool HExpandable { get { return true; } }
		public bool VExpandable {
			get {
				foreach (Gtk.Widget w in Children) {
					WidgetSite site = (WidgetSite)w;

					if (!site.VExpandable)
						return false;
				}
				return true;
			}
		}

		public event OccupancyChangedHandler OccupancyChanged;

		private void ChildOccupancyChanged (IDesignTimeContainer container)
		{
			if (OccupancyChanged != null)
				OccupancyChanged (this);
		}
	}
}
