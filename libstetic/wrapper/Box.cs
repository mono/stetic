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
			RegisterWrapper (typeof (Stetic.Wrapper.Box),
					 BoxProperties,
					 Widget.CommonWidgetProperties);

			BoxChildProperties = new ItemGroup ("Box Child Layout",
							    typeof (Gtk.Box.BoxChild),
							    "PackType",
							    "Position",
							    "Expand",
							    "Fill",
							    "Padding");
			RegisterChildItems (typeof (Stetic.Wrapper.Box),
					    BoxChildProperties);

			ItemGroup contextMenu = new ItemGroup (null,
							       typeof (Stetic.Wrapper.Box),
							       typeof (Gtk.Box),
							       "InsertBefore",
							       "InsertAfter");
			RegisterContextMenu (typeof (Stetic.Wrapper.Box), contextMenu);
		}

		protected Box (IStetic stetic, Gtk.Box box) : base (stetic, box)
		{
			for (int i = 0; i < 3; i++)
				box.PackStart (CreateWidgetSite ());
		}

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
