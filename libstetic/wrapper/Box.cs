using System;
using System.Collections;

namespace Stetic.Wrapper {

	public abstract class Box : Stetic.Wrapper.Container {
		public static ItemGroup BoxProperties;

		static Box () {
			BoxProperties = new ItemGroup ("Box Properties",
						       typeof (Gtk.Box),
						       "Homogeneous",
						       "Spacing",
						       "BorderWidth");
			RegisterWrapper (typeof (Stetic.Wrapper.Box),
					 BoxProperties,
					 Widget.CommonWidgetProperties);

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

		protected override void SiteOccupancyChanged (WidgetSite site) {
			Gtk.Box.BoxChild bc = ((Gtk.Box)Wrapped)[site] as Gtk.Box.BoxChild;
			bc.Expand = bc.Fill = (this is HBox) ? site.HExpandable : site.VExpandable;
			base.SiteOccupancyChanged (site);
		}


		public class BoxChild : Stetic.Wrapper.Container.ContainerChild {
			public static ItemGroup BoxChildProperties;

			static BoxChild ()
			{
				BoxChildProperties = new ItemGroup ("Box Child Layout",
								    typeof (Gtk.Box.BoxChild),
								    "PackType",
								    "Position",
								    "Expand",
								    "Fill",
								    "Padding");
				BoxChildProperties["Fill"].DependsOn (BoxChildProperties["Expand"]);
				RegisterWrapper (typeof (Stetic.Wrapper.Box.BoxChild),
						 BoxChildProperties);
			}

			public BoxChild (IStetic stetic, Gtk.Box.BoxChild bc) : base (stetic, bc) {}
		}
	}
}
