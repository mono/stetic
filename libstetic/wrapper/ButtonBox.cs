using System;

namespace Stetic.Wrapper {

	public abstract class ButtonBox : Box {
		public static ItemGroup ButtonBoxProperties;
		public static ItemGroup ButtonBoxChildProperties;

		static ButtonBox () {
			ButtonBoxProperties = new ItemGroup ("Button Box Properties",
							     typeof (Gtk.ButtonBox),
							     "LayoutStyle",
							     "Homogeneous",
							     "Spacing",
							     "BorderWidth");
			ButtonBoxChildProperties = new ItemGroup ("Button Box Child Layout",
								  typeof (Gtk.ButtonBox.ButtonBoxChild),
								  "Secondary");

			groups = new ItemGroup[] {
				ButtonBox.ButtonBoxProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new ItemGroup[] {
				ButtonBox.ButtonBoxChildProperties
			};
		}

		protected ButtonBox (IStetic stetic, Gtk.ButtonBox bbox) : base (stetic, bbox) {}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }

		static ItemGroup[] childgroups;
		public override ItemGroup[] ChildItemGroups { get { return childgroups; } }
	}
}
