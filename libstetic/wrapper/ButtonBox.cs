using System;

namespace Stetic.Wrapper {

	public abstract class ButtonBox : Box {

		public static new Type WrappedType = typeof (Gtk.ButtonBox);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Button Box Properties",
				      "LayoutStyle",
				      "Homogeneous",
				      "Spacing",
				      "BorderWidth");
		}

		public class ButtonBoxChild : Container.ContainerChild {

			public static new Type WrappedType = typeof (Gtk.ButtonBox.ButtonBoxChild);

			static new void Register (Type type)
			{
				ItemGroup props = AddItemGroup (type, "Button Box Child Layout",
								"PackType",
								"Secondary",
								"Position",
								"Expand",
								"Fill",
								"Padding");
				props["Fill"].DependsOn (props["Expand"]);
			}
		}
	}
}
