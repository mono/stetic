using System;
using System.Collections;

namespace Stetic.Wrapper {

	public abstract class Box : Container {

		public static new Type WrappedType = typeof (Gtk.Box);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Box Properties",
				      "Homogeneous",
				      "Spacing",
				      "BorderWidth");

			AddContextMenuItems (type,
					     "InsertBefore",
					     "InsertAfter");
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				for (int i = 0; i < 3; i++)
					box.PackStart (CreateWidgetSite ());
			}
			box.SizeAllocated += box_SizeAllocated;
			box.ParentSet += box_ParentSet;
		}

		Gtk.Box box {
			get {
				return (Gtk.Box)Wrapped;
			}
		}

		protected override void Sync ()
		{
			WidgetSite site = box.Parent as WidgetSite;
			if (site == null)
				return;

			Gtk.Widget[] children = box.Children;
			WidgetSite[] sorted = new WidgetSite[children.Length];

			foreach (Gtk.Widget child in children) {
				WidgetSite childsite = child as WidgetSite;
				if (childsite == null || !childsite.Occupied)
					continue;
				Gtk.Box.BoxChild bc = box[child] as Gtk.Box.BoxChild;
				sorted[bc.Position] = childsite;
			}

			Gdk.Rectangle alloc = box.Allocation;

			site.ClearFaults ();
			if (sorted.Length == 0)
				return;

			if (this is HBox) {
				if (sorted[0] != null)
					site.AddVFault (0, 0, 0, alloc.Height);
				if (sorted[sorted.Length - 1] != null)
					site.AddVFault (sorted.Length, alloc.Width, 0, alloc.Height);
			} else {
				if (sorted[0] != null)
					site.AddHFault (0, 0, 0, alloc.Width);
				if (sorted[sorted.Length - 1] != null)
					site.AddHFault (sorted.Length, alloc.Height, 0, alloc.Width);
			}

			for (int i = 1; i < sorted.Length; i++) {
				if (sorted[i - 1] != null && sorted[i] != null) {
					Gdk.Rectangle alloc1 = sorted[i - 1].Allocation;
					Gdk.Rectangle alloc2 = sorted[i].Allocation;

					if (this is HBox)
						site.AddVFault (i, (alloc1.X + alloc1.Width + alloc2.X) / 2, 0, alloc.Height);
					else
						site.AddHFault (i, (alloc1.Y + alloc1.Height + alloc2.Y) / 2, 0, alloc.Width);
				}
			}
		}

		[Command ("Insert Before", "Insert an empty row/column before the selected one")]
		void InsertBefore (IWidgetSite context)
		{
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

		[Command ("Insert After", "Insert an empty row/column after the selected one")]
		void InsertAfter (IWidgetSite context)
		{
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
			if (AutoSize[site]) {
				Gtk.Box.BoxChild bc = ((Gtk.Box)Wrapped)[site] as Gtk.Box.BoxChild;
				bc.Expand = bc.Fill = (this is HBox) ? site.HExpandable : site.VExpandable;
			}
			base.SiteOccupancyChanged (site);
		}

		void box_ParentSet (object obj, Gtk.ParentSetArgs args)
		{
			WidgetSite site = box.Parent as WidgetSite;
			if (site == null)
				return;

			site.DropOn += DropOn;
		}

		void box_SizeAllocated (object obj, Gtk.SizeAllocatedArgs args)
		{
			Sync ();
		}

		void DropOn (Gtk.Widget w, object faultId)
		{
			WidgetSite site = CreateWidgetSite ();
			box.PackStart (site);
			site.Add (w);
			box.ReorderChild (site, (int)faultId);

			Sync ();
		}

		public class BoxChild : Container.ContainerChild {

			public static new Type WrappedType = typeof (Gtk.Box.BoxChild);

			static new void Register (Type type)
			{
				ItemGroup props = AddItemGroup (type, "Box Child Layout",
								"PackType",
								"Position",
								"AutoSize",
								"Expand",
								"Fill",
								"Padding");
				props["Expand"].DependsInverselyOn (props["AutoSize"]);
				props["Fill"].DependsInverselyOn (props["AutoSize"]);
				props["Fill"].DependsOn (props["Expand"]);
			}

			protected override void EmitNotify (string propertyName)
			{
				if (propertyName == "AutoSize") {
					base.EmitNotify ("Expand");
					base.EmitNotify ("Fill");
				}
				base.EmitNotify (propertyName);
			}
		}
	}
}
