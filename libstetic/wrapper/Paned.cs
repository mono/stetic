using System;

namespace Stetic.Wrapper {

	public abstract class Paned : Container {

		public static new Type WrappedType = typeof (Gtk.Paned);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Pane Properties",
				      "MinPosition",
				      "MaxPosition",
				      "BorderWidth");
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				Gtk.Paned paned = (Gtk.Paned)Wrapped;
				paned.Pack1 (CreatePlaceholder (), true, false);
				paned.Pack2 (CreatePlaceholder (), true, false);
			}
		}

		protected override void ReplaceChild (Gtk.Widget oldChild, Gtk.Widget newChild)
		{
			Gtk.Paned paned = (Gtk.Paned)Wrapped;

			paned.Remove (oldChild);
			if (oldChild == paned.Child1)
				paned.Add1 (newChild);
			else if (oldChild == paned.Child2)
				paned.Add2 (newChild);
		}

		public class PanedChild : Container.ContainerChild {

			public static new Type WrappedType = typeof (Gtk.Paned.PanedChild);

			static new void Register (Type type)
			{
				AddItemGroup (type, "Pane Child Layout",
					      "Resize",
					      "Shrink");
			}
		}
	}
}
