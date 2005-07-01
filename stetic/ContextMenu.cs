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

			item = LabelItem (ph);
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
				item = LabelItem (context);
				item.Sensitive = false;
				Add (item);
			}

			item = new MenuItem ("_Select");
			item.Activated += DoSelect;
			Add (item);

			ClassDescriptor klass = Registry.LookupClass (wrapper.Wrapped.GetType ());
			if (klass != null) {
				foreach (CommandDescriptor cmd in klass.ContextMenu.Items) {
					item = new MenuItem (cmd.Label);
					if (cmd.Enabled (wrapper.Wrapped, context)) {
						item.Activated += delegate (object o, EventArgs args) {
							cmd.Run (wrapper.Wrapped, context);
						};
					} else
						item.Sensitive = false;
					Add (item);
				}
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

					item = LabelItem (parentWrapper.Wrapped);
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

		void DoSelect (object obj, EventArgs args)
		{
			wrapper.Select ();
		}

		void DoDelete (object obj, EventArgs args)
		{
			wrapper.Delete ();
		}

		static MenuItem LabelItem (Gtk.Widget widget)
		{
			ImageMenuItem item;
			Label label;

			label = new Label (widget is Placeholder ? "Placeholder" : widget.Name);
			label.UseUnderline = false;
			label.SetAlignment (0.0f, 0.5f);
			item = new ImageMenuItem ();
			item.Add (label);

			ClassDescriptor klass = Registry.LookupClass (widget.GetType ());
			if (klass != null) {
				Gdk.Pixbuf pixbuf = klass.Icon;
				int width, height;
				Gtk.Icon.SizeLookup (Gtk.IconSize.Menu, out width, out height);
				item.Image = new Gtk.Image (pixbuf.ScaleSimple (width, height, Gdk.InterpType.Bilinear));
			}

			return item;
		}
	}
}
