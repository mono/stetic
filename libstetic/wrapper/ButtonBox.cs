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
			RegisterWrapper (typeof (Stetic.Wrapper.ButtonBox),
					 ButtonBoxProperties,
					 Widget.CommonWidgetProperties);

			ButtonBoxChildProperties = new ItemGroup ("Button Box Child Layout",
								  typeof (Gtk.ButtonBox.ButtonBoxChild),
								  "Secondary");
			RegisterChildItems (typeof (Stetic.Wrapper.ButtonBox),
					    ButtonBoxChildProperties);
		}

		protected ButtonBox (IStetic stetic, Gtk.ButtonBox bbox) : base (stetic, bbox) {}
	}
}
