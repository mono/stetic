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
		}

		protected Gtk.Box box {
			get {
				return (Gtk.Box)Wrapped;
			}
		}

		protected override void DoSync ()
		{
			if (!box.IsRealized)
				return;

			DND.ClearFaults (this);

			Gtk.Widget[] children = box.Children;
			if (children.Length == 0)
				return;

			Gtk.Widget[] sorted = new Gtk.Widget[children.Length];

			foreach (Gtk.Widget child in children) {
				Gtk.Box.BoxChild bc = box[child] as Gtk.Box.BoxChild;
				if (AutoSize[child]) {
					bool exp = (this is HBox) ? ChildHExpandable (child) : ChildVExpandable (child);
					if (bc.Expand != exp)
						bc.Expand = exp;
					if (bc.Fill != exp)
						bc.Fill = exp;
				}

				if (!(child is Placeholder))
					sorted[bc.Position] = child;
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
			Gtk.Widget widget = child.Wrapped;

			if (widget != null && AutoSize[widget]) {
				Gtk.Box.BoxChild bc = box[widget] as Gtk.Box.BoxChild;
				bc.Expand = bc.Fill = (this is HBox) ? ChildHExpandable (widget) : ChildVExpandable (widget);
			}
			base.ChildContentsChanged (child);
		}

		protected override void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
		{
			base.ReplaceChild (oldChild, newChild);

			Container container = Stetic.Wrapper.Container.Lookup (newChild);
			if (container != null)
				ChildContentsChanged (container);
		}

		void box_SizeAllocated (object obj, Gtk.SizeAllocatedArgs args)
		{
			Sync ();
		}

		public override void Drop (Gtk.Widget w, object faultId)
		{
			AutoSize[w] = true;
			box.PackStart (w);
			box.ReorderChild (w, (int)faultId);
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
