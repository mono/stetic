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
				box.PackStart (CreateWidgetSite ());
				box.PackStart (CreateWidgetSite ());
			}
			box.SizeAllocated += box_SizeAllocated;
			box.ParentSet += box_ParentSet;
			box_ParentSet (this, null);
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

			site.ClearFaults ();

			Gtk.Widget[] children = box.Children;
			if (children.Length == 0)
				return;

			WidgetSite[] sorted = new WidgetSite[children.Length];

			foreach (Gtk.Widget child in children) {
				WidgetSite childsite = child as WidgetSite;
				if (childsite == null || childsite.Child == null)
					continue;
				Gtk.Box.BoxChild bc = box[child] as Gtk.Box.BoxChild;
				sorted[bc.Position] = childsite;
			}

			if (this is HBox || this is HButtonBox) {
				if (sorted[0] != null)
					site.AddVFault (0, null, sorted[0]);
				if (sorted[sorted.Length - 1] != null)
					site.AddVFault (sorted.Length, sorted[sorted.Length - 1], null);
			} else {
				if (sorted[0] != null)
					site.AddHFault (0, null, sorted[0]);
				if (sorted[sorted.Length - 1] != null)
					site.AddHFault (sorted.Length, sorted[sorted.Length - 1], null);
			}

			for (int i = 1; i < sorted.Length; i++) {
				if (sorted[i - 1] != null && sorted[i] != null) {
					if (this is HBox || this is HButtonBox)
						site.AddVFault (i, sorted[i - 1], sorted[i]);
					else
						site.AddHFault (i, sorted[i - 1], sorted[i]);
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
			EmitContentsChanged ();
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
			EmitContentsChanged ();
		}

		protected override void SiteShapeChanged (WidgetSite site) {
			if (AutoSize[site]) {
				Gtk.Box.BoxChild bc = ((Gtk.Box)Wrapped)[site] as Gtk.Box.BoxChild;
				bc.Expand = bc.Fill = (this is HBox) ? site.HExpandable : site.VExpandable;
			}
			base.SiteShapeChanged (site);
		}

		protected override void SiteOccupancyChanged (WidgetSite site) {
			if (!site.Occupied) {
				site.Destroy ();
				return;
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
			EmitContentsChanged ();

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
