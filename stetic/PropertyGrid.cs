using Gtk;
using Gdk;
using GLib;
using System;
using System.Reflection;

namespace Stetic {

	public class PropertyGrid : Gtk.Table {

		public PropertyGrid () : base (1, 2, false)
		{
			RowSpacing = ColumnSpacing = BorderWidth = 2;
			NoSelection ();
		}

		protected void Clear ()
		{
			foreach (Widget w in Children)
				Remove (w);
			Resize (1, 2);
		}

		protected void Append (string labelText, Widget rep)
		{
			uint row;

			row = NRows;
			if (row == 1 && Children.Length == 0)
				row = 0;

			Label label = new Label (labelText);
			label.UseMarkup = true;
			label.Justify = Justification.Left;
			label.Xalign = 0;
			label.Show ();
			Attach (label, 0, 1, row, row + 1,
				AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);

			if (rep != null) {
				rep.ShowAll ();
				Attach (rep, 1, 2, row, row + 1,
					AttachOptions.Expand | AttachOptions.Fill, 0, 0, 0);
			}
		}

		public void NoSelection ()
		{
			Clear ();
			Append ("<i>No selection</i>", null);
		}

		public void Select (WidgetBox wbox)
		{
			Clear ();

			Widget w = wbox.Child;
			if (w == null)
				return;

			foreach (PropertyInfo info in w.GetType().GetProperties (BindingFlags.Instance | BindingFlags.Public)) {
				foreach (object attr in info.GetCustomAttributes (false)) {
					PropertyAttribute pattr = attr as PropertyAttribute;
					if (pattr == null)
						continue;

					ParamSpec pspec = ParamSpec.LookupObjectProperty (w, pattr.Name);
					Append (pspec.Nick, PropertyEditors.MakeEditor (pspec, info, w));
				}
			}
		}
	}

	public class ChildPropertyGrid : PropertyGrid {

		public new void Select (WidgetBox wbox)
		{
			Clear ();

			Widget w = wbox.Child;
			if (w == null)
				return;

			Container parent = wbox.Parent as Container;
			if (parent == null)
				return;

			ContainerChild cc = parent[wbox];

			foreach (PropertyInfo info in cc.GetType().GetProperties (BindingFlags.Instance | BindingFlags.Public)) {
				foreach (object attr in info.GetCustomAttributes (false)) {
					ChildPropertyAttribute pattr = attr as ChildPropertyAttribute;
					if (pattr == null)
						continue;

					ParamSpec pspec = ParamSpec.LookupChildProperty (parent, pattr.Name);
					Append (pspec.Nick, PropertyEditors.MakeEditor (pspec, info, cc));
				}
			}
		}
	}
}
