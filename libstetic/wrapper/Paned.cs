using System;

namespace Stetic.Wrapper {

	public abstract class Paned : Stetic.Wrapper.Container {
		public static PropertyGroup PanedProperties;
		public static PropertyGroup PanedChildProperties;

		static Paned () {
			PanedProperties = new PropertyGroup ("Pane Properties",
							     typeof (Gtk.Paned),
							     "MinPosition",
							     "MaxPosition",
							     "BorderWidth");

			PanedChildProperties = new PropertyGroup ("Pane Child Layout",
								  typeof (Gtk.Paned.PanedChild),
								  "Resize",
								  "Shrink");

			groups = new PropertyGroup[] {
				Paned.PanedProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[] {
				Paned.PanedChildProperties
			};
		}

		protected Paned (IStetic stetic, Gtk.Paned paned) : base (stetic, paned)
		{
			paned.Pack1 (CreateWidgetSite (), true, false);
			paned.Pack2 (CreateWidgetSite (), true, false);
		}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public override PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }
	}
}
