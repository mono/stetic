using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Toggle Button", "togglebutton.png", ObjectWrapperType.Widget)]
	public class ToggleButton : Button {

		public static new Type WrappedType = typeof (Gtk.ToggleButton);

		static new void Register (Type type)
		{
			ItemGroup props = AddItemGroup (type, "Toggle Button Properties",
							"UseStock",
							"StockId",
							"Label",
							"Active",
							"Inconsistent");
			props["StockId"].DependsOn (props["UseStock"]);
			props["Label"].DependsInverselyOn (props["UseStock"]);
		}

		protected override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				Gtk.ToggleButton button = (Gtk.ToggleButton)Wrapped;
				button.Label = button.Name;
			}
		}
	}
}
