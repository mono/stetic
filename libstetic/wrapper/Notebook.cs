using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Notebook", "notebook.png", ObjectWrapperType.Container)]
	public class Notebook : Stetic.Wrapper.Container, Stetic.IContextMenuProvider {

		public static PropertyGroup NotebookProperties;
		public static PropertyGroup NotebookChildProperties;

		static Notebook () {
			NotebookProperties = new PropertyGroup ("Notebook Properties",
								typeof (Gtk.Notebook),
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
				Stetic.Wrapper.Widget.CommonWidgetProperties
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

		public Notebook (IStetic stetic) : this (stetic, new Gtk.Notebook ()) {}

		public Notebook (IStetic stetic, Gtk.Notebook notebook) : base (stetic, notebook)
		{
			notebook.AppendPage (CreateWidgetSite (), new Gtk.Label ("page"));
		}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public override PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		private Gtk.Notebook notebook {
			get {
				return (Gtk.Notebook)Wrapped;
			}
		}

		public IEnumerable ContextMenuItems (IWidgetSite context)
		{
			ContextMenuItem[] items;
			int page = notebook.PageNum ((Gtk.Widget)context);

			// FIXME; I'm only assigning to a variable rather than
			// returning it directly to make emacs indentation happy
			items = new ContextMenuItem[] {
				new ContextMenuItem ("Go to Previous Page", new ContextMenuItemDelegate (PreviousPage), page > 0),
				new ContextMenuItem ("Go to Next Page", new ContextMenuItemDelegate (NextPage), page < notebook.NPages - 1),
				new ContextMenuItem ("Delete Page", new ContextMenuItemDelegate (DeletePage)),
				new ContextMenuItem ("Swap with Previous Page", new ContextMenuItemDelegate (SwapPrevious), page > 0),
				new ContextMenuItem ("Swap with Next Page", new ContextMenuItemDelegate (SwapNext), page < notebook.NPages - 1),
				new ContextMenuItem ("Insert Page Before", new ContextMenuItemDelegate (InsertBefore)),
				new ContextMenuItem ("Insert Page After", new ContextMenuItemDelegate (InsertAfter)),
			};
			return items;
		}

		void PreviousPage (IWidgetSite context)
		{
			notebook.PrevPage ();
		}

		void NextPage (IWidgetSite context)
		{
			notebook.NextPage ();
		}

		void DeletePage (IWidgetSite context)
		{
			notebook.RemovePage (notebook.CurrentPage);
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
			notebook.CurrentPage = notebook.InsertPage (CreateWidgetSite (), new Gtk.Label ("page"), notebook.CurrentPage);
		}

		void InsertAfter (IWidgetSite context)
		{
			notebook.CurrentPage = notebook.InsertPage (CreateWidgetSite (), new Gtk.Label ("page"), notebook.CurrentPage + 1);
		}

		public override bool HExpandable {
			get {
				foreach (WidgetSite site in Sites) {
					if (site.HExpandable) 
						return true;
				}
				return false;
			}
		}

		public override bool VExpandable {
			get {
				foreach (WidgetSite site in Sites) {
					if (site.VExpandable)
						return true;
				}
				return false;
			}
		}
	}
}
