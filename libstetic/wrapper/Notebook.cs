using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Notebook", "notebook.png", typeof (Gtk.Notebook), ObjectWrapperType.Container)]
	public class Notebook : Stetic.Wrapper.Container {

		public static ItemGroup NotebookProperties;

		static Notebook () {
			NotebookProperties = new ItemGroup ("Notebook Properties",
							    typeof (Stetic.Wrapper.Notebook),
							    typeof (Gtk.Notebook),
							    "EnablePopup",
							    "Homogeneous",
							    "TabPos",
							    "TabHborder",
							    "TabVborder",
							    "ShowBorder",
							    "ShowTabs",
							    "Scrollable",
							    "BorderWidth",
							    "InsertBefore",
							    "InsertAfter");
			RegisterWrapper (typeof (Stetic.Wrapper.Notebook),
					 NotebookProperties,
					 Widget.CommonWidgetProperties);

			ItemGroup contextMenu = new ItemGroup (null,
							       typeof (Stetic.Wrapper.Notebook),
							       typeof (Gtk.Notebook),
							       "PreviousPage",
							       "NextPage",
							       "DeletePage",
							       "SwapPrevious",
							       "SwapNext",
							       "InsertBefore",
							       "InsertAfter");
			RegisterContextMenu (typeof (Stetic.Wrapper.Notebook), contextMenu);
		}

		public Notebook (IStetic stetic) : this (stetic, new Gtk.Notebook (), false) {}


		public Notebook (IStetic stetic, Gtk.Notebook notebook, bool initialized) : base (stetic, notebook, initialized)
		{
			if (!initialized) {
				InsertPage (0);
			}
		}

		private Gtk.Notebook notebook {
			get {
				return (Gtk.Notebook)Wrapped;
			}
		}

		int InsertPage (int position)
		{
			WidgetSite pageSite, labelSite;

			pageSite = CreateWidgetSite ();
			labelSite = CreateWidgetSite ();
			labelSite.Add (new Stetic.Wrapper.Label (stetic).Wrapped as Gtk.Widget);
			return notebook.InsertPage (pageSite, labelSite, position);
		}

		[Command ("Go to Previous Page", "Show the previous page", "CheckPreviousPage")]
		void PreviousPage ()
		{
			notebook.PrevPage ();
		}

		bool CheckPreviousPage ()
		{
			return notebook.CurrentPage > 0;
		}

		[Command ("Go to Next Page", "Show the next page", "CheckNextPage")]
		void NextPage ()
		{
			notebook.NextPage ();
		}

		bool CheckNextPage ()
		{
			return notebook.CurrentPage < notebook.NPages - 1;
		}

		[Command ("Delete Page", "Delete the current page")]
		void DeletePage ()
		{
			notebook.RemovePage (notebook.CurrentPage);
		}

		[Command ("Swap with Previous Page", "Swap the contents of this page with the contents of the previous page", "CheckPreviousPage")]
		void SwapPrevious ()
		{
			// FIXME
		}

		[Command ("Swap with Next Page", "Swap the contents of this page with the contents of the next page", "CheckNextPage")]
		void SwapNext ()
		{
			// FIXME
		}

		[Command ("Insert Page Before", "Insert a blank page before this one")]
		void InsertBefore ()
		{
			notebook.CurrentPage = InsertPage (notebook.CurrentPage);
		}

		[Command ("Insert Page After", "Insert a blank page after this one")]
		void InsertAfter ()
		{
			notebook.CurrentPage = InsertPage (notebook.CurrentPage + 1);
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

		public class NotebookChild : Stetic.Wrapper.Container.ContainerChild {
			public static ItemGroup NotebookChildProperties;

			static NotebookChild ()
			{
				NotebookChildProperties = new ItemGroup ("Notebook Child Layout",
									 typeof (Gtk.Notebook.NotebookChild),
									 "TabLabel",
									 "Position",
									 "TabPack",
									 "TabExpand",
									 "TabFill",
									 "MenuLabel");
				RegisterWrapper (typeof (Stetic.Wrapper.Notebook.NotebookChild),
						 NotebookChildProperties);
			}

			public NotebookChild (IStetic stetic, Gtk.Notebook.NotebookChild notebookchild, bool initialized) : base (stetic, notebookchild, initialized) {}
		}
	}
}
