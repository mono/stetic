using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Toggle Button", "togglebutton.png", typeof (Gtk.ToggleButton), ObjectWrapperType.Widget)]
	public class ToggleButton : Button {

		public static ItemGroup ToggleButtonProperties;

		static ToggleButton () {
			ToggleButtonProperties = new ItemGroup ("Toggle Button Properties",
								typeof (Stetic.Wrapper.ToggleButton),
								typeof (Gtk.ToggleButton),
								"UseStock",
								"StockId",
								"Label",
								"Active",
								"Inconsistent");
			ToggleButtonProperties["StockId"].DependsOn (ToggleButtonProperties["UseStock"]);
			ToggleButtonProperties["Label"].DependsInverselyOn (ToggleButtonProperties["UseStock"]);
			RegisterWrapper (typeof (Stetic.Wrapper.ToggleButton),
					 ToggleButtonProperties,
					 Button.ButtonExtraProperties,
					 Widget.CommonWidgetProperties);
		}

		public ToggleButton (IStetic stetic) : this (stetic, new Gtk.ToggleButton (), false) {}


		public ToggleButton (IStetic stetic, Gtk.ToggleButton button, bool initialized) : base (stetic, button, initialized)
		{
			if (!initialized) {
				button.Label = button.Name;
			}
		}
	}
}
