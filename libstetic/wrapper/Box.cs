using System;
using System.Collections;

namespace Stetic.Wrapper {

	public abstract class Box : Stetic.Wrapper.Container {
		public static ItemGroup BoxProperties;
		public static ItemGroup BoxChildProperties;

		static Box () {
			BoxProperties = new ItemGroup ("Box Properties",
						       typeof (Gtk.Box),
						       "Homogeneous",
						       "Spacing",
						       "BorderWidth");
			BoxChildProperties = new ItemGroup ("Box Child Layout",
							    typeof (Gtk.Box.BoxChild),
							    "PackType",
							    "Position",
							    "Expand",
							    "Fill",
							    "Padding");

			groups = new ItemGroup[] {
				Box.BoxProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new ItemGroup[] {
				Box.BoxChildProperties
			};

			contextItems = new ItemGroup (null,
						      typeof (Stetic.Wrapper.Box),
						      typeof (Gtk.Box),
						      "InsertBefore",
						      "InsertAfter");
		}

		protected Box (IStetic stetic, Gtk.Box box) : base (stetic, box)
		{
			for (int i = 0; i < 3; i++)
				box.PackStart (CreateWidgetSite ());
		}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }

		static ItemGroup[] childgroups;
		public override ItemGroup[] ChildItemGroups { get { return childgroups; } }

		static ItemGroup contextItems;
		public override ItemGroup ContextMenuItems { get { return contextItems; } }

		[Command ("Insert Before")]
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

		[Command ("Insert After")]
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
