using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Notebook", "notebook.png", ObjectWrapperType.Container)]
	public class Notebook : Container {

		public static new Type WrappedType = typeof (Gtk.Notebook);

		internal static new void Register (Type type)
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

		ArrayList tabs = new ArrayList ();

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized)
				InsertPage (0);
		}

		public override Widget GladeImportChild (string className, string id, Hashtable props, Hashtable childprops)
		{
			if (childprops.Count == 1 && ((string)childprops["type"]) == "tab") {
				ObjectWrapper wrapper = Stetic.ObjectWrapper.GladeImport (stetic, className, id, props);
				Gtk.Widget widget = (Gtk.Widget)wrapper.Wrapped;
				notebook.SetTabLabel (notebook.GetNthPage (notebook.NPages - 1), widget);
				tabs.Add (widget);
				return (Widget)wrapper;
			} else
				return base.GladeImportChild (className, id, props, childprops);
		}

		public override void GladeExportChild (Widget wrapper, out string className,
						       out string internalId, out string id,
						       out Hashtable props,
						       out Hashtable childprops)
		{
			Gtk.Widget widget = wrapper.Wrapped as Gtk.Widget;
			if (!tabs.Contains (widget)) {
				base.GladeExportChild (wrapper, out className, out internalId,
						       out id, out props, out childprops);
				return;
			}

			internalId = null;
			childprops = new Hashtable ();
			childprops["type"] = "tab";
			wrapper.GladeExport (out className, out id, out props);
		}

		private Gtk.Notebook notebook {
			get {
				return (Gtk.Notebook)Wrapped;
			}
		}

		public override void Select (Stetic.Wrapper.Widget wrapper)
		{
			int index = tabs.IndexOf (wrapper.Wrapped);
			if (index != -1 && index != notebook.CurrentPage)
				notebook.CurrentPage = index;
			base.Select (wrapper);
		}

		protected override void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
		{
			int index = tabs.IndexOf (oldChild);
			if (index != -1) {
				tabs[index] = newChild;
				Gtk.Widget page = notebook.GetNthPage (index);
				notebook.SetTabLabel (page, newChild);
			} else {
				Gtk.Widget tab = notebook.GetTabLabel (oldChild);
				int current = notebook.CurrentPage;
				base.ReplaceChild (oldChild, newChild);
				notebook.CurrentPage = current;
				notebook.SetTabLabel (newChild, tab);
			}
		}

		int InsertPage (int position)
		{
			Gtk.Widget widget;

			Stetic.Wrapper.Label label = new Stetic.Wrapper.Label ("page" + (notebook.NPages + 1).ToString ());
			widget = label.Wrapped;
			tabs.Insert (position, widget);

			return notebook.InsertPage (CreatePlaceholder (), widget, position);
		}

		[Command ("Go to Previous Page", "Show the previous page", "CheckPreviousPage")]
		internal void PreviousPage ()
		{
			notebook.PrevPage ();
		}

		internal bool CheckPreviousPage ()
		{
			return notebook.CurrentPage > 0;
		}

		[Command ("Go to Next Page", "Show the next page", "CheckNextPage")]
		internal void NextPage ()
		{
			notebook.NextPage ();
		}

		internal bool CheckNextPage ()
		{
			return notebook.CurrentPage < notebook.NPages - 1;
		}

		[Command ("Delete Page", "Delete the current page")]
		internal void DeletePage ()
		{
			tabs.RemoveAt (notebook.CurrentPage);
			notebook.RemovePage (notebook.CurrentPage);
		}

		[Command ("Swap with Previous Page", "Swap the contents of this page with the contents of the previous page", "CheckPreviousPage")]
		internal void SwapPrevious ()
		{
			// FIXME
		}

		[Command ("Swap with Next Page", "Swap the contents of this page with the contents of the next page", "CheckNextPage")]
		internal void SwapNext ()
		{
			// FIXME
		}

		[Command ("Insert Page Before", "Insert a blank page before this one")]
		internal void InsertBefore ()
		{
			notebook.CurrentPage = InsertPage (notebook.CurrentPage);
		}

		[Command ("Insert Page After", "Insert a blank page after this one")]
		internal void InsertAfter ()
		{
			notebook.CurrentPage = InsertPage (notebook.CurrentPage + 1);
		}

		public override bool HExpandable {
			get {
				foreach (Gtk.Widget w in notebook) {
					if (ChildHExpandable (w)) 
						return true;
				}
				return false;
			}
		}

		public override bool VExpandable {
			get {
				foreach (Gtk.Widget w in notebook) {
					if (ChildVExpandable (w))
						return true;
				}
				return false;
			}
		}

		public class NotebookChild : Container.ContainerChild {

			public static new Type WrappedType = typeof (Gtk.Notebook.NotebookChild);

			internal static new void Register (Type type)
			{
				AddItemGroup (type,
					      "Notebook Child Layout",
					      "Position",
					      "TabPack",
					      "TabExpand",
					      "TabFill",
					      "MenuLabel");
			}
		}
	}
}
