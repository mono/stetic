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
				WidgetSite site = CreateWidgetSite ((Gtk.Widget)wrapper.Wrapped);
// FIXME				site.Selected += LabelSelected;

				notebook.SetTabLabel (notebook.GetNthPage (notebook.NPages - 1), site);
				tabs.Add (site);
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
			if (!tabs.Contains (widget.Parent)) {
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

		int InsertPage (int position)
		{
			WidgetSite labelSite;

			Stetic.Wrapper.Label label = new Stetic.Wrapper.Label ("page" + (notebook.NPages + 1).ToString ());
			labelSite = CreateWidgetSite ((Gtk.Widget)label.Wrapped);
// FIXME			labelSite.Selected += LabelSelected;
			tabs.Insert (position, labelSite);

			return notebook.InsertPage (CreatePlaceholder (), labelSite, position);
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
			tabs.RemoveAt (notebook.CurrentPage);
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

		void LabelSelected (object obj, EventArgs args)
		{
			WidgetSite site = obj as WidgetSite;
			int index = tabs.IndexOf (site);
			if (index != -1 && index != notebook.CurrentPage) {
				notebook.CurrentPage = index;
				site.GrabFocus ();
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
