using Gtk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class ContextMenu : Gtk.Menu {

		IWidgetSite site;

		public ContextMenu (Placeholder ph)
		{
			MenuItem item;

			this.site = null;

			item = LabelItem ("Placeholder");
			item.Sensitive = false;
			Add (item);

			item = new MenuItem ("_Select");
			item.Sensitive = false;
			Add (item);

			Widget w = ph;
			while (w.Parent != null && !(w is WidgetSite))
				w = w.Parent;
			if (w is WidgetSite)
				BuildContextMenu ((WidgetSite)w, false, false, ph);
			else
				BuildContextMenu (WindowSite.LookupSite (w), false, false, ph);
		}

		public ContextMenu (IWidgetSite site) : this (site, site as Gtk.Widget) {}

		public ContextMenu (IWidgetSite site, Gtk.Widget context)
		{
			MenuItem item;

			this.site = site;

			item = LabelItem (site.Contents.Name);
			item.Sensitive = false;
			Add (item);

			item = new MenuItem ("_Select");
			item.Activated += DoSelect;
			Add (item);

			Stetic.Wrapper.Object wrapper = Stetic.Wrapper.Object.Lookup (site.Contents);
			if (wrapper != null) {
				foreach (CommandDescriptor cmd in wrapper.ContextMenuItems.Items) {
					item = new MenuItem (cmd.Label);
					if (cmd.Enabled (wrapper, context))
						item.Activated += new Stupid69614Workaround (cmd, wrapper, context).Activate;
					else
						item.Sensitive = false;
					Add (item);
				}
			}

			BuildContextMenu (site.ParentSite, true, site.Internal, context);
		}

		void BuildContextMenu (IWidgetSite parentSite, bool occupied, bool isInternal, Widget context)
		{
			MenuItem item;

			item = new MenuItem ("Cu_t");
			if (!occupied || isInternal)
				item.Sensitive = false;
			Add (item);
			item = new MenuItem ("_Copy");
			if (!occupied)
				item.Sensitive = false;
			Add (item);
			item = new MenuItem ("_Paste");
			if (occupied)
				item.Sensitive = false;
			Add (item);
			item = new MenuItem ("_Delete");
			if (occupied && !isInternal)
				item.Activated += DoDelete;
			else
				item.Sensitive = false;
			Add (item);

			for (; parentSite != null; parentSite = parentSite.ParentSite) {
				Add (new SeparatorMenuItem ());

				item = LabelItem (parentSite.Contents.Name);
				item.Submenu = new ContextMenu (parentSite, context);
				Add (item);
			}

			ShowAll ();
		}

		private class Stupid69614Workaround {
			CommandDescriptor cmd;
			ObjectWrapper wrapper;
			Widget context;

			public Stupid69614Workaround (CommandDescriptor cmd, ObjectWrapper wrapper, Widget context)
			{
				this.cmd = cmd;
				this.wrapper = wrapper;
				this.context = context;
			}

			public void Activate (object o, EventArgs args) {
				cmd.Run (wrapper, context);
			}
		}

		void DoSelect (object obj, EventArgs args)
		{
			site.Select ();
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
