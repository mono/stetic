using System;
using System.Collections;

namespace Stetic.Wrapper {

	public abstract class Box : Stetic.Wrapper.Container, Stetic.IContextMenuProvider {
		public static PropertyGroup BoxProperties;
		public static PropertyGroup BoxChildProperties;

		static Box () {
			BoxProperties = new PropertyGroup ("Box Properties",
							   typeof (Gtk.Box),
							   "Homogeneous",
							   "Spacing",
							   "BorderWidth");
			BoxChildProperties = new PropertyGroup ("Box Child Layout",
								typeof (Gtk.Box.BoxChild),
								"PackType",
								"Position",
								"Expand",
								"Fill",
								"Padding");

			groups = new PropertyGroup[] {
				Box.BoxProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[] {
				Box.BoxChildProperties
			};
		}

		protected Box (IStetic stetic, Gtk.Box box) : base (stetic, box)
		{
			for (int i = 0; i < 3; i++)
				box.PackStart (CreateWidgetSite ());
		}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public override PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		public IEnumerable ContextMenuItems (IWidgetSite context)
		{
			ContextMenuItem[] items;

			// FIXME; I'm only assigning to a variable rather than
			// returning it directly to make emacs indentation happy
			items = new ContextMenuItem[] {
				new ContextMenuItem ("Insert Before", new ContextMenuItemDelegate (InsertBefore)),
				new ContextMenuItem ("Insert After", new ContextMenuItemDelegate (InsertAfter)),
			};
			return items;
		}

		void InsertBefore (IWidgetSite context)
		{
			Gtk.Box box = (Gtk.Box)Wrapped;
			Gtk.Box.BoxChild bc = box[(Gtk.Widget)context] as Gtk.Box.BoxChild;
			WidgetSite site = CreateWidgetSite ();
			if (bc.PackType == Gtk.PackType.Start) {
				box.PackStart (site);
				box.ReorderChild (site, bc.Position);
			} else {
				box.PackEnd (site);
				box.ReorderChild (site, bc.Position + 1);
			}
		}

		void InsertAfter (IWidgetSite context)
		{
			Gtk.Box box = (Gtk.Box)Wrapped;
			Gtk.Box.BoxChild bc = box[(Gtk.Widget)context] as Gtk.Box.BoxChild;
			WidgetSite site = CreateWidgetSite ();
			if (bc.PackType == Gtk.PackType.Start) {
				box.PackStart (site);
				box.ReorderChild (site, bc.Position + 1);
			} else {
				box.PackEnd (site);
				box.ReorderChild (site, bc.Position);
			}
		}
	}
}
