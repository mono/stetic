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
				box.PackStart (CreatePlaceholder ());
				box.PackStart (CreatePlaceholder ());
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

		protected override void DoSync ()
		{
			WidgetSite site = box.Parent as WidgetSite;
			if (site == null)
				return;

			DND.ClearFaults (this);

			Gtk.Widget[] children = box.Children;
			if (children.Length == 0)
				return;

			WidgetSite[] sorted = new WidgetSite[children.Length];

			foreach (Gtk.Widget child in children) {
				WidgetBox wbox = child as WidgetBox;
				if (wbox == null)
					continue;

				Gtk.Box.BoxChild bc = box[child] as Gtk.Box.BoxChild;
				if (AutoSize[wbox]) {
					bool exp = (this is HBox) ? wbox.HExpandable : wbox.VExpandable;
					if (bc.Expand != exp)
						bc.Expand = exp;
					if (bc.Fill != exp)
						bc.Fill = exp;
				}

				WidgetSite childsite = child as WidgetSite;
				if (childsite != null)
					sorted[bc.Position] = childsite;
			}

			if (this is HBox || this is HButtonBox) {
				if (sorted[0] != null)
					DND.AddVFault (this, 0, null, sorted[0]);
				if (sorted[sorted.Length - 1] != null)
					DND.AddVFault (this, sorted.Length, sorted[sorted.Length - 1], null);
			} else {
				if (sorted[0] != null)
					DND.AddHFault (this, 0, null, sorted[0]);
				if (sorted[sorted.Length - 1] != null)
					DND.AddHFault (this, sorted.Length, sorted[sorted.Length - 1], null);
			}

			for (int i = 1; i < sorted.Length; i++) {
				if (sorted[i - 1] != null && sorted[i] != null) {
					if (this is HBox || this is HButtonBox)
						DND.AddVFault (this, i, sorted[i - 1], sorted[i]);
					else
						DND.AddHFault (this, i, sorted[i - 1], sorted[i]);
				}
			}
		}

		[Command ("Insert Before", "Insert an empty row/column before the selected one")]
		void InsertBefore (Gtk.Widget context)
		{
			Gtk.Box.BoxChild bc = box[context] as Gtk.Box.BoxChild;
			Placeholder ph = CreatePlaceholder ();
			if (bc.PackType == Gtk.PackType.Start) {
				box.PackStart (ph);
				box.ReorderChild (ph, bc.Position);
			} else {
				box.PackEnd (ph);
				box.ReorderChild (ph, bc.Position + 1);
			}
			EmitContentsChanged ();
		}

		[Command ("Insert After", "Insert an empty row/column after the selected one")]
		void InsertAfter (Gtk.Widget context)
		{
			Gtk.Box.BoxChild bc = box[context] as Gtk.Box.BoxChild;
			Placeholder ph = CreatePlaceholder ();
			if (bc.PackType == Gtk.PackType.Start) {
				box.PackStart (ph);
				box.ReorderChild (ph, bc.Position + 1);
			} else {
				box.PackEnd (ph);
				box.ReorderChild (ph, bc.Position);
			}
			EmitContentsChanged ();
		}

		protected override void ChildContentsChanged (Container child) {
			WidgetSite site = child.Wrapped.Parent as WidgetSite;

			if (site != null && AutoSize[site]) {
				Gtk.Box.BoxChild bc = box[site] as Gtk.Box.BoxChild;
				bc.Expand = bc.Fill = (this is HBox) ? site.HExpandable : site.VExpandable;
			}
			base.ChildContentsChanged (child);
		}

		protected override void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
		{
			base.ReplaceChild (oldChild, newChild);

			WidgetSite newSite = newChild as WidgetSite;
			if (newSite != null) {
				Container container = Stetic.Wrapper.Container.Lookup (newSite.Child);
				if (container != null)
					ChildContentsChanged (container);
			}
		}

		void box_ParentSet (object obj, Gtk.ParentSetArgs args)
		{
			WidgetSite site = box.Parent as WidgetSite;
			if (site == null)
				return;
		}

		void box_SizeAllocated (object obj, Gtk.SizeAllocatedArgs args)
		{
			Sync ();
		}

		public override void Drop (Gtk.Widget w, object faultId)
		{
			WidgetSite site = CreateWidgetSite (w);
			AutoSize[site] = true;
			box.PackStart (site);
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
