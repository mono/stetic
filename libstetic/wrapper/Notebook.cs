using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Notebook", "notebook.png", ObjectWrapperType.Container)]
	public class Notebook : Stetic.Wrapper.Container {

		public static ItemGroup NotebookProperties;
		public static ItemGroup NotebookChildProperties;

		static Notebook () {
			NotebookProperties = new ItemGroup ("Notebook Properties",
							    typeof (Stetic.Wrapper.Notebook),
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
							    "BorderWidth",
							    "InsertBefore",
							    "InsertAfter");
			RegisterWrapper (typeof (Stetic.Wrapper.Notebook),
					 NotebookProperties,
					 Widget.CommonWidgetProperties);

			NotebookChildProperties = new ItemGroup ("Notebook Child Layout",
								 typeof (Gtk.Notebook.NotebookChild),
								 "TabLabel",
								 "Position",
								 "TabPack",
								 "TabExpand",
								 "TabFill",
								 "MenuLabel");
			RegisterChildItems (typeof (Stetic.Wrapper.Notebook),
					    NotebookChildProperties);

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

		public Notebook (IStetic stetic) : this (stetic, new Gtk.Notebook ()) {}

		public Notebook (IStetic stetic, Gtk.Notebook notebook) : base (stetic, notebook)
		{
			InsertPage (0);
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

		[Command ("Go to Previous Page", "CheckPreviousPage")]
		void PreviousPage ()
		{
			notebook.PrevPage ();
		}

		bool CheckPreviousPage ()
		{
			return notebook.CurrentPage > 0;
		}

		[Command ("Go to Next Page", "CheckNextPage")]
		void NextPage ()
		{
			notebook.NextPage ();
		}

		bool CheckNextPage ()
		{
			return notebook.CurrentPage < notebook.NPages - 1;
		}

		[Command ("Delete Page")]
		void DeletePage ()
		{
			notebook.RemovePage (notebook.CurrentPage);
		}

		[Command ("Swap with Previous Page", "CheckPreviousPage")]
		void SwapPrevious ()
		{
			// FIXME
		}

		[Command ("Swap with Next Page", "CheckNextPage")]
		void SwapNext ()
		{
			// FIXME
		}

		[Command ("Insert Page Before")]
		void InsertBefore ()
		{
			notebook.CurrentPage = InsertPage (notebook.CurrentPage);
		}

		[Command ("Insert Page After")]
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
	}
}
