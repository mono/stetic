using Gtk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class ContextMenu : Gtk.Menu {

		Gtk.Widget widget;

		public ContextMenu (Placeholder ph)
		{
			MenuItem item;

			this.widget = ph;

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

			widget = wrapper.Wrapped;

			if (widget == context) {
				item = LabelItem (widget);
				item.Sensitive = false;
				Add (item);
			}

			item = new MenuItem ("_Select");
			item.Activated += DoSelect;
			Add (item);

			ClassDescriptor klass = wrapper.ClassDescriptor;
			if (klass != null) {
				foreach (ItemDescriptor id in klass.ContextMenu) {
					CommandDescriptor cmd = (CommandDescriptor)id;
					item = new MenuItem (cmd.Label);
					if (cmd.Enabled (widget, context)) {
						Gtk.Widget wdup = widget, cdup = context; // FIXME bxc 75689
						item.Activated += delegate (object o, EventArgs args) {
							cmd.Run (wdup, cdup);
						};
					} else
						item.Sensitive = false;
					Add (item);
				}
			}

			BuildContextMenu (wrapper.ParentWrapper, true, wrapper.InternalChildProperty != null, widget == context, context);
		}

		void BuildContextMenu (Stetic.Wrapper.Widget parentWrapper, bool occupied, bool isInternal, bool top, Widget context)
		{
			MenuItem item;

			item = new ImageMenuItem (Gtk.Stock.Cut, null);
			if (occupied && !isInternal)
				item.Activated += DoCut;
			else
				item.Sensitive = false;
			Add (item);

			item = new ImageMenuItem (Gtk.Stock.Copy, null);
			if (occupied)
				item.Activated += DoCopy;
			else
				item.Sensitive = false;
			Add (item);

			item = new ImageMenuItem (Gtk.Stock.Paste, null);
			if (!occupied)
				item.Activated += DoPaste;
			else
				item.Sensitive = false;
			Add (item);

			item = new ImageMenuItem (Gtk.Stock.Delete, null);
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
			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (widget);
			if (wrapper != null)
				wrapper.Select ();
		}

		void DoCut (object obj, EventArgs args)
		{
			Clipboard.Cut (widget);
		}

		void DoCopy (object obj, EventArgs args)
		{
			Clipboard.Copy (widget);
		}

		void DoPaste (object obj, EventArgs args)
		{
			Clipboard.Paste (widget as Placeholder);
		}

		void DoDelete (object obj, EventArgs args)
		{
			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (widget);
			if (wrapper != null)
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

			Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (widget);
			
			if (wrapper != null) {
				ClassDescriptor klass = wrapper.ClassDescriptor;
				if (klass != null) {
					Gdk.Pixbuf pixbuf = klass.Icon;
					int width, height;
					Gtk.Icon.SizeLookup (Gtk.IconSize.Menu, out width, out height);
					item.Image = new Gtk.Image (pixbuf.ScaleSimple (width, height, Gdk.InterpType.Bilinear));
				}
			}
			
			return item;
		}
	}
}
