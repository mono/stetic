using System;

namespace Stetic.Wrapper {

	public abstract class Paned : Stetic.Wrapper.Container {
		public static ItemGroup PanedProperties;

		static Paned () {
			PanedProperties = new ItemGroup ("Pane Properties",
							 typeof (Gtk.Paned),
							 "MinPosition",
							 "MaxPosition",
							 "BorderWidth");
			RegisterWrapper (typeof (Stetic.Wrapper.Paned),
					 PanedProperties,
					 Widget.CommonWidgetProperties);
		}


		protected Paned (IStetic stetic, Gtk.Paned paned, bool initialized) : base (stetic, paned, initialized)
		{
			if (!initialized) {
				paned.Pack1 (CreateWidgetSite (), true, false);
				paned.Pack2 (CreateWidgetSite (), true, false);
			}
		}

		public class PanedChild : Stetic.Wrapper.Container.ContainerChild {
			public static ItemGroup PanedChildProperties;

			static PanedChild ()
			{
				PanedChildProperties = new ItemGroup ("Pane Child Layout",
								      typeof (Gtk.Paned.PanedChild),
								      "Resize",
								      "Shrink");
				RegisterWrapper (typeof (Stetic.Wrapper.Paned.PanedChild),
						 PanedChildProperties);
			}

			public PanedChild (IStetic stetic, Gtk.Paned.PanedChild bc, bool initialized) : base (stetic, bc, false) {}
		}
	}
}
