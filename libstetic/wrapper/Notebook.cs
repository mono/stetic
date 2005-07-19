using System;
using System.Collections;
using System.Xml;

namespace Stetic.Wrapper {

	public class Notebook : Container {

		ArrayList tabs = new ArrayList ();

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized)
				InsertPage (0);
		}

		public override Widget GladeImportChild (XmlElement child_elem)
		{
			if ((string)GladeUtils.GetChildProperty (child_elem, "type", "") == "tab") {
				ObjectWrapper wrapper = Stetic.ObjectWrapper.GladeImport (proj, child_elem["widget"]);
				Gtk.Widget widget = (Gtk.Widget)wrapper.Wrapped;
				notebook.SetTabLabel (notebook.GetNthPage (notebook.NPages - 1), widget);
				tabs.Add (widget);
				return (Widget)wrapper;
			} else
				return base.GladeImportChild (child_elem);
		}

		public override XmlElement GladeExportChild (Widget wrapper, XmlDocument doc)
		{
			XmlElement child_elem = base.GladeExportChild (wrapper, doc);
			if (tabs.Contains (wrapper.Wrapped))
				GladeUtils.SetChildProperty (child_elem, "type", "tab");
			return child_elem;
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

		internal void PreviousPage ()
		{
			notebook.PrevPage ();
		}

		internal bool CheckPreviousPage ()
		{
			return notebook.CurrentPage > 0;
		}

		internal void NextPage ()
		{
			notebook.NextPage ();
		}

		internal bool CheckNextPage ()
		{
			return notebook.CurrentPage < notebook.NPages - 1;
		}

		internal void DeletePage ()
		{
			tabs.RemoveAt (notebook.CurrentPage);
			notebook.RemovePage (notebook.CurrentPage);
		}

		internal void SwapPrevious ()
		{
			// FIXME
		}

		internal void SwapNext ()
		{
			// FIXME
		}

		internal void InsertBefore ()
		{
			notebook.CurrentPage = InsertPage (notebook.CurrentPage);
		}

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
	}
}
