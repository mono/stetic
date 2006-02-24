using System;
using System.Collections;
using System.Xml;

namespace Stetic.Wrapper {

	public class Notebook : Container {

		ArrayList tabs = new ArrayList ();

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				if (notebook.Children.Length != 0) {
					// Remove the dummy page Container.Wrap added
					notebook.Remove (notebook.Children[0]);
				}
				InsertPage (0);
			}
		}

		protected override Widget ReadChild (XmlElement child_elem, FileFormat format)
		{
			if ((string)GladeUtils.GetChildProperty (child_elem, "type", "") == "tab") {
				ObjectWrapper wrapper = Stetic.ObjectWrapper.Read (proj, child_elem["widget"], format);
				Gtk.Widget widget = (Gtk.Widget)wrapper.Wrapped;
				notebook.SetTabLabel (notebook.GetNthPage (notebook.NPages - 1), widget);
				tabs.Add (widget);
				return (Widget)wrapper;
			} else
				return base.ReadChild (child_elem, format);
		}

		protected override XmlElement WriteChild (Widget wrapper, XmlDocument doc, FileFormat format)
		{
			XmlElement child_elem = base.WriteChild (wrapper, doc, format);
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
			if (wrapper != null) {
				int index = tabs.IndexOf (wrapper.Wrapped);
				if (index != -1 && index != notebook.CurrentPage)
					notebook.CurrentPage = index;
			}
			base.Select (wrapper);
		}

		public override void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
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
			Gtk.Label label = (Gtk.Label)Registry.NewInstance ("Gtk.Label", proj);
			label.LabelProp = "page" + (notebook.NPages + 1).ToString ();
			tabs.Insert (position, label);

			return notebook.InsertPage (CreatePlaceholder (), label, position);
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
