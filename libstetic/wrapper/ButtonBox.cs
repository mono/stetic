using System;

namespace Stetic.Wrapper {

	public abstract class ButtonBox : Box {
		public static PropertyGroup ButtonBoxProperties;
		public static PropertyGroup ButtonBoxChildProperties;

		static ButtonBox () {
			ButtonBoxProperties = new PropertyGroup ("Button Box Properties",
								 typeof (Gtk.ButtonBox),
								 "LayoutStyle",
								 "Homogeneous",
								 "Spacing",
								 "BorderWidth");
			ButtonBoxChildProperties = new PropertyGroup ("Button Box Child Layout",
								      typeof (Gtk.ButtonBox.ButtonBoxChild),
								      "Secondary");

			groups = new PropertyGroup[] {
				ButtonBox.ButtonBoxProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[] {
				ButtonBox.ButtonBoxChildProperties
			};
		}

		protected ButtonBox (IStetic stetic, Gtk.ButtonBox bbox) : base (stetic, bbox) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public override PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }
	}
}
