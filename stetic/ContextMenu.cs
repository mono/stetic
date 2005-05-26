using Gtk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class ContextMenu : Gtk.Menu {

		Stetic.Wrapper.Widget wrapper;

		public ContextMenu (Placeholder ph)
		{
			MenuItem item;

			this.wrapper = null;

			item = LabelItem ("Placeholder", null);
			item.Sensitive = false;
			Add (item);

			item = new MenuItem ("_Select");
			item.Sensitive = false;
			Add (item);

			BuildContextMenu (Stetic.Wrapper.Container.LookupParent (ph), false, false, true, ph);
		}

		public ContextMenu (Stetic.Wrapper.Widget wrapper) : this (wrapper, wrapper.Wrapped) {}

		public ContextMenu (Stetic.Wrapper.Widget wrapper, Gtk.Widget context)
		{
			MenuItem item;

			this.wrapper = wrapper;

			if (context == wrapper.Wrapped) {
				item = LabelItem (wrapper.Wrapped.Name, wrapper.GetType ());
				item.Sensitive = false;
				Add (item);
			}

			item = new MenuItem ("_Select");
			item.Activated += DoSelect;
			Add (item);

			foreach (CommandDescriptor cmd in wrapper.ContextMenuItems.Items) {
				item = new MenuItem (cmd.Label);
				if (cmd.Enabled (wrapper, context))
					item.Activated += new Stupid69614Workaround (cmd, wrapper, context).Activate;
				else
					item.Sensitive = false;
				Add (item);
			}

			BuildContextMenu (wrapper.ParentWrapper, true, wrapper.InternalChildId != null, context == wrapper.Wrapped, context);
		}

		void BuildContextMenu (Stetic.Wrapper.Widget parentWrapper, bool occupied, bool isInternal, bool top, Widget context)
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

			if (top) {
				for (; parentWrapper != null; parentWrapper = parentWrapper.ParentWrapper) {
					Add (new SeparatorMenuItem ());

					item = LabelItem (parentWrapper.Wrapped.Name, parentWrapper.GetType ());
					item.Submenu = new ContextMenu (parentWrapper, context);
					Add (item);
				}
			}

			ShowAll ();
		}

		protected override void OnSelectionDone ()
		{
			Destroy ();
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
			wrapper.Select ();
		}

		void DoDelete (object obj, EventArgs args)
		{
			wrapper.Delete ();
		}

		static MenuItem LabelItem (string labelString, Type type)
		{
			ImageMenuItem item;
			Label label;

			label = new Label (labelString);
			label.UseUnderline = false;
			label.SetAlignment (0.0f, 0.5f);
			item = new ImageMenuItem ();
			item.Add (label);

			if (type != null) {
				Gdk.Pixbuf pixbuf = Palette.IconForType (type);
				int width, height;
				Gtk.Icon.SizeLookup (Gtk.IconSize.Menu, out width, out height);
				item.Image = new Gtk.Image (pixbuf.ScaleSimple (width, height, Gdk.InterpType.Bilinear));
			}

			return item;
		}
	}
}
