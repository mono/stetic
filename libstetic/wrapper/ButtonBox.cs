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
		}

		protected ButtonBox (IStetic stetic, Gtk.ButtonBox bbox) : base (stetic, bbox) {}

		public class ButtonBoxChild : Stetic.Wrapper.Box.BoxChild {
			public static ItemGroup ButtonBoxChildProperties;

			static ButtonBoxChild ()
			{
				ButtonBoxChildProperties = new ItemGroup ("Button Box Child Layout",
									  typeof (Gtk.ButtonBox.ButtonBoxChild),
									  "PackType",
									  "Secondary",
									  "Position",
									  "Expand",
									  "Fill",
									  "Padding");
				RegisterWrapper (typeof (Stetic.Wrapper.ButtonBox.ButtonBoxChild),
						 ButtonBoxChildProperties);
			}

			public ButtonBoxChild (IStetic stetic, Gtk.ButtonBox.ButtonBoxChild bbc) : base (stetic, bbc) {}
		}
	}
}
