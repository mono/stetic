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
				paned.Pack1 (CreateWidgetSite (), true, false);
				paned.Pack2 (CreateWidgetSite (), true, false);
			}
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
