using System;

namespace Stetic.Wrapper {

	public abstract class Paned : Stetic.Wrapper.Container {
		public static ItemGroup PanedProperties;
		public static ItemGroup PanedChildProperties;

		static Paned () {
			PanedProperties = new ItemGroup ("Pane Properties",
							 typeof (Gtk.Paned),
							 "MinPosition",
							 "MaxPosition",
							 "BorderWidth");
			RegisterWrapper (typeof (Stetic.Wrapper.Paned),
					 PanedProperties,
					 Widget.CommonWidgetProperties);

			PanedChildProperties = new ItemGroup ("Pane Child Layout",
							      typeof (Gtk.Paned.PanedChild),
							      "Resize",
							      "Shrink");
			RegisterChildItems (typeof (Stetic.Wrapper.Paned),
					    PanedChildProperties);
		}

		protected Paned (IStetic stetic, Gtk.Paned paned) : base (stetic, paned)
		{
			paned.Pack1 (CreateWidgetSite (), true, false);
			paned.Pack2 (CreateWidgetSite (), true, false);
		}
	}
}
