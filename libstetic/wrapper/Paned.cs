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
			
			PanedChildProperties = new ItemGroup ("Pane Child Layout",
							      typeof (Gtk.Paned.PanedChild),
							      "Resize",
							      "Shrink");

			groups = new ItemGroup[] {
				Paned.PanedProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new ItemGroup[] {
				Paned.PanedChildProperties
			};
		}

		protected Paned (IStetic stetic, Gtk.Paned paned) : base (stetic, paned)
		{
			paned.Pack1 (CreateWidgetSite (), true, false);
			paned.Pack2 (CreateWidgetSite (), true, false);
		}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }

		static ItemGroup[] childgroups;
		public override ItemGroup[] ChildItemGroups { get { return childgroups; } }
	}
}
