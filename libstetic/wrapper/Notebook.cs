using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Notebook", "notebook.png", ObjectWrapperType.Container)]
	public class Notebook : Container {

		public static new Type WrappedType = typeof (Gtk.Notebook);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Notebook Properties",
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

			AddContextMenuItems (type,
					     "PreviousPage",
					     "NextPage",
					     "DeletePage",
					     "SwapPrevious",
					     "SwapNext",
					     "InsertBefore",
					     "InsertAfter");
		}

		protected override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized)
				InsertPage (0);
		}

		private Gtk.Notebook notebook {
			get {
				return (Gtk.Notebook)Wrapped;
			}
		}

		int InsertPage (int position)
		{
			WidgetSite pageSite;

			pageSite = CreateWidgetSite ();
			return notebook.InsertPage (pageSite, null /* FIXME */, position);
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

		public class NotebookChild : Container.ContainerChild {

			public static new Type WrappedType = typeof (Gtk.Notebook.NotebookChild);

			static new void Register (Type type)
			{
				AddItemGroup (type,
					      "Notebook Child Layout",
					      "TabLabel",
					      "Position",
					      "TabPack",
					      "TabExpand",
					      "TabFill",
					      "MenuLabel");
			}
		}
	}
}
