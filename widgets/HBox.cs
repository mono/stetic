using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;

namespace Stetic.Wrapper {

	[WidgetWrapper ("HBox", "hbox.png", WidgetType.Container)]
	public class HBox : Gtk.HBox, Stetic.IContainerWrapper, Stetic.IContextMenuProvider {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static HBox () {
			groups = new PropertyGroup[] {
				Box.BoxProperties,
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[] {
				Box.BoxChildProperties
			};
		}

		public HBox () : base (false, 0)
		{
			for (int i = 0; i < 3; i++) {
				WidgetSite site = new WidgetSite ();
				site.OccupancyChanged += SiteOccupancyChanged;
				PackStart (site);
			}
		}

		public IEnumerable ContextMenuItems (IWidgetSite context)
		{
			ContextMenuItem[] items;

			// FIXME; I'm only assigning to a variable rather than
			// returning it directly to make emacs indentation happy
			items = new ContextMenuItem[] {
				new ContextMenuItem ("Insert Before", new ContextMenuItemDelegate (InsertBefore)),
				new ContextMenuItem ("Insert After", new ContextMenuItemDelegate (InsertAfter)),
			};
			return items;
		}

		void InsertBefore (IWidgetSite context)
		{
			Gtk.Box.BoxChild bc = this[(Gtk.Widget)context] as Gtk.Box.BoxChild;
			WidgetSite site = new WidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			site.Show ();
			if (bc.PackType == PackType.Start) {
				PackStart (site);
				ReorderChild (site, bc.Position);
			} else {
				PackEnd (site);
				ReorderChild (site, bc.Position + 1);
			}
		}

		void InsertAfter (IWidgetSite context)
		{
			Gtk.Box.BoxChild bc = this[(Gtk.Widget)context] as Gtk.Box.BoxChild;
			WidgetSite site = new WidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			site.Show ();
			if (bc.PackType == PackType.Start) {
				PackStart (site);
				ReorderChild (site, bc.Position + 1);
			} else {
				PackEnd (site);
				ReorderChild (site, bc.Position);
			}
		}

		public bool HExpandable {
			get {
				foreach (Gtk.Widget w in Children) {
					WidgetSite site = (WidgetSite)w;

					if (site.HExpandable) 
						return true;
				}
				return false;
			}
		}

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

		public event ExpandabilityChangedHandler ExpandabilityChanged;

		private void SiteOccupancyChanged (WidgetSite site)
		{
			if (ExpandabilityChanged != null)
				ExpandabilityChanged (this);
		}
	}
}
