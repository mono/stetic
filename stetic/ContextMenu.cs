using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class ContextMenu : Gtk.Menu {

		WidgetSite site;

		public ContextMenu (WidgetSite site, bool top)
		{
			MenuItem item;

			this.site = site;

			if (top) {
				item = LabelItem (site.Occupied ? site.Child.Name : "Placeholder");
				item.Sensitive = false;
				Add (item);
			}

			item = new MenuItem ("_Select");
			item.Activated += DoSelect;
			Add (item);
			item = new MenuItem ("Cu_t");
			if (!site.Occupied)
				item.Sensitive = false;
			Add (item);
			item = new MenuItem ("_Copy");
			if (!site.Occupied)
				item.Sensitive = false;
			Add (item);
			item = new MenuItem ("_Paste");
			if (site.Occupied)
				item.Sensitive = false;
			Add (item);
			item = new MenuItem ("_Delete");
			if (site.Occupied)
				item.Activated += DoDelete;
			else
				item.Sensitive = false;
			Add (item);

			if (top) {
				while (site != null) {
					Widget w = site.Parent;
					if (w == null)
						break;
					site = w.Parent as WidgetSite;
					if (site == null)
						break;

					Add (new SeparatorMenuItem ());

					item = LabelItem (w.Name);
					item.Submenu = new ContextMenu (site, false);
					Add (item);
				}
			}

			ShowAll ();
		}

		void DoSelect (object obj, EventArgs args)
		{
			site.GrabFocus ();
		}

		void DoDelete (object obj, EventArgs args)
		{
			site.Delete ();
		}

		static MenuItem LabelItem (string labelString)
		{
			MenuItem item;
			Label label;

			label = new Label (labelString);
			label.UseUnderline = false;
			label.SetAlignment (0.0f, 0.5f);
			item = new MenuItem ();
			item.Add (label);

			return item;
		}
	}
}
