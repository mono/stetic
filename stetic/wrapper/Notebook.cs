using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Notebook", "notebook.png", WidgetType.Container)]
	public class Notebook : Gtk.Notebook, Stetic.IContainerWrapper, Stetic.IContextMenuProvider {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup NotebookProperties;

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		public static PropertyGroup NotebookChildProperties;

		static Notebook () {
			NotebookProperties = new PropertyGroup ("Notebook Properties",
								typeof (Stetic.Wrapper.Notebook),
								"EnablePopup",
								"Homogeneous",
								"TabPos",
								"TabBorder",
								"TabHborder",
								"TabVborder",
								"ShowBorder",
								"ShowTabs",
								"Scrollable",
								"BorderWidth");

			groups = new PropertyGroup[] {
				NotebookProperties,
				Widget.CommonWidgetProperties
			};

			NotebookChildProperties = new PropertyGroup ("Notebook Child Layout",
								     typeof (Gtk.Notebook.NotebookChild),
								     "TabLabel",
								     "Position",
								     "TabPack",
								     "TabExpand",
								     "TabFill",
								     "MenuLabel");

			childgroups = new PropertyGroup[] {
				NotebookChildProperties
			};
		}

		public Notebook ()
		{
			WidgetSite site = new WidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			AppendPage (site, new Label ("page"));
		}

		public IEnumerable ContextMenuItems (IWidgetSite context)
		{
			ContextMenuItem[] items;
			int page = PageNum ((Gtk.Widget)context);

			// FIXME; I'm only assigning to a variable rather than
			// returning it directly to make emacs indentation happy
			items = new ContextMenuItem[] {
				new ContextMenuItem ("Go to Previous Page", new ContextMenuItemDelegate (PreviousPage), page > 0),
				new ContextMenuItem ("Go to Next Page", new ContextMenuItemDelegate (NextPage), page < NPages - 1),
				new ContextMenuItem ("Delete Page", new ContextMenuItemDelegate (DeletePage)),
				new ContextMenuItem ("Swap with Previous Page", new ContextMenuItemDelegate (SwapPrevious), page > 0),
				new ContextMenuItem ("Swap with Next Page", new ContextMenuItemDelegate (SwapNext), page < NPages - 1),
				new ContextMenuItem ("Insert Page Before", new ContextMenuItemDelegate (InsertBefore)),
				new ContextMenuItem ("Insert Page After", new ContextMenuItemDelegate (InsertAfter)),
			};
			return items;
		}

		void PreviousPage (IWidgetSite context)
		{
			PrevPage ();
		}

		void NextPage (IWidgetSite context)
		{
			NextPage ();
		}

		void DeletePage (IWidgetSite context)
		{
			RemovePage (CurrentPage);
		}

		void SwapPrevious (IWidgetSite context)
		{
			// FIXME
		}

		void SwapNext (IWidgetSite context)
		{
			// FIXME
		}

		void InsertBefore (IWidgetSite context)
		{
			WidgetSite site = new WidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			site.Show ();
			CurrentPage = InsertPage (site, new Label ("page"), CurrentPage);
		}

		void InsertAfter (IWidgetSite context)
		{
			WidgetSite site = new WidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			site.Show ();
			CurrentPage = InsertPage (site, new Label ("page"), CurrentPage + 1);
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

					if (site.VExpandable)
						return true;
				}
				return false;
			}
		}

		public event ExpandabilityChangedHandler ExpandabilityChanged;

		private void SiteOccupancyChanged (WidgetSite site)
		{
			if (ExpandabilityChanged != null)
				ExpandabilityChanged (this);
		}

		protected override void OnRemoved (Gtk.Widget w)
		{
			WidgetSite site = w as WidgetSite;

			if (site == null)
				return;

			site.OccupancyChanged -= SiteOccupancyChanged;
			base.OnRemoved (w);
		}

	}
}
