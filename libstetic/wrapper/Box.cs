using System;
using System.Collections;

namespace Stetic.Wrapper {

	public class Box : Container {

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

		protected override bool AllowPlaceholders {
			get {
				return InternalChildProperty != null;
			}
		}

		// DoSync() does two things: first, it makes sure that all of the
		// PackStart widgets have Position numbers less than all of the
		// PackEnd widgets. Second, it creates faults anywhere two widgets
		// could be split apart. The fault IDs correspond to the Position
		// a widget would have to be assigned to end up in that slot
		// (negated for PackEnd slots).
		//
		// Position/PackType:   0S 1S 2S     4E  3E
		//                    +----------------------+
		//                    | AA BB CC     DD  EE  |
		//                    +----------------------+
		// Fault Id:           0  1  2  3  -5  -4  -3

		protected override void DoSync ()
		{
			if (!box.IsRealized)
				return;

			DND.ClearFaults (this);

			Gtk.Widget[] children = box.Children;
			if (children.Length == 0)
				return;

			Gtk.Widget[] sorted = new Gtk.Widget[children.Length];
			int last_start = -1;
			bool hbox = ContainerOrientation == Gtk.Orientation.Horizontal;

			foreach (Gtk.Widget child in children) {
				Gtk.Box.BoxChild bc = box[child] as Gtk.Box.BoxChild;
				if (AutoSize[child]) {
					bool exp = hbox ? ChildHExpandable (child) : ChildVExpandable (child);
					if (bc.Expand != exp)
						bc.Expand = exp;
					if (bc.Fill != exp)
						bc.Fill = exp;
				}

				// Make sure all of the PackStart widgets are before
				// any PackEnd widgets in the list.
				if (bc.PackType == Gtk.PackType.Start) {
					if (bc.Position != ++last_start) {
						Array.Copy (sorted, last_start, sorted, last_start + 1, bc.Position - last_start);
						box.ReorderChild (child, last_start);
					}
				}

				if (!(child is Placeholder))
					sorted[bc.Position] = child;
			}

			// The orientation of the faults is the opposite of the
			// orientation of the box
			Gtk.Orientation orientation = hbox ? Gtk.Orientation.Vertical : orientation = Gtk.Orientation.Horizontal;
			Gtk.SideType before = hbox ? Gtk.SideType.Left : Gtk.SideType.Top;
			Gtk.SideType after = hbox ? Gtk.SideType.Right : Gtk.SideType.Bottom;

			// If there are no PackStart widgets, we need a fault at the leading
			// edge. Otherwise if there's a widget at the leading edge, we need a
			// fault before it.
			if (last_start == -1)
				DND.AddFault (this, 0, before, null);
			else if (sorted[0] != null)
				DND.AddFault (this, 0, before, sorted[0]);

			// Add a fault between each pair of (non-placeholder) start widgets
			for (int i = 1; i <= last_start; i++) {
				if (sorted[i - 1] != null && sorted[i] != null)
					DND.AddFault (this, i, orientation, sorted[i - 1], sorted[i]);
			}

			// If there's a non-placeholder at the end of the PackStart
			// range, add a fault after it
			if (last_start > -1 && sorted[last_start] != null)
				DND.AddFault (this, last_start + 1, after, sorted[last_start]);

			// Now the PackEnd widgets
			if (last_start == sorted.Length - 1)
				DND.AddFault (this, -(last_start + 1), after, null);
			else if (sorted[last_start + 1] != null)
				DND.AddFault (this, -(last_start + 1), after, sorted[last_start + 1]);

			for (int i = last_start + 2; i < sorted.Length; i++) {
				if (sorted[i - 1] != null && sorted[i] != null)
					DND.AddFault (this, -i, orientation, sorted[i - 1], sorted[i]);
			}

			if (sorted.Length > last_start + 1 && sorted[sorted.Length - 1] != null)
				DND.AddFault (this, -sorted.Length, before, sorted[sorted.Length - 1]);
		}

		internal void InsertBefore (Gtk.Widget context)
		{
			int position;
			Gtk.PackType type;

			if (context == box) {
				position = 0;
				type = Gtk.PackType.Start;
			} else {
				Gtk.Box.BoxChild bc = (Gtk.Box.BoxChild)ContextChildProps (context);
				position = bc.Position;
				type = bc.PackType;
			}

			Placeholder ph = CreatePlaceholder ();
			if (type == Gtk.PackType.Start) {
				box.PackStart (ph);
				box.ReorderChild (ph, position);
			} else {
				box.PackEnd (ph);
				box.ReorderChild (ph, position + 1);
			}
			EmitContentsChanged ();
		}

		internal void InsertAfter (Gtk.Widget context)
		{
			int position;
			Gtk.PackType type;

			if (context == box) {
				position = 0;
				type = Gtk.PackType.End;
			} else {
				Gtk.Box.BoxChild bc = (Gtk.Box.BoxChild)ContextChildProps (context);
				position = bc.Position;
				type = bc.PackType;
			}

			Placeholder ph = CreatePlaceholder ();
			if (type == Gtk.PackType.Start) {
				box.PackStart (ph);
				box.ReorderChild (ph, position + 1);
			} else {
				box.PackEnd (ph);
				box.ReorderChild (ph, position);
			}
			EmitContentsChanged ();
		}

		protected override void ChildContentsChanged (Container child) {
			Gtk.Widget widget = child.Wrapped;

			if (widget != null && AutoSize[widget]) {
				Gtk.Box.BoxChild bc = box[widget] as Gtk.Box.BoxChild;
				bc.Expand = bc.Fill = (ContainerOrientation == Gtk.Orientation.Horizontal) ? ChildHExpandable (widget) : ChildVExpandable (widget);
			}
			base.ChildContentsChanged (child);
		}

		public override void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
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
			int pos = (int)faultId;

			Freeze ();
			if (pos >= 0) {
				box.PackStart (w);
				box.ReorderChild (w, pos);
			} else {
				box.PackEnd (w);
				box.ReorderChild (w, -pos);
			}
			EmitContentsChanged ();
			Thaw ();
		}

		public class BoxChild : Container.ContainerChild {

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
