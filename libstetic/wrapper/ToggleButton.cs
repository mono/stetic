using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Toggle Button", "togglebutton.png", ObjectWrapperType.Widget)]
	public class ToggleButton : Button {

		public static PropertyGroup ToggleButtonProperties;

		static ToggleButton () {
			ToggleButtonProperties = new PropertyGroup ("Toggle Button Properties",
								    typeof (Stetic.Wrapper.ToggleButton),
								    typeof (Gtk.ToggleButton),
								    "UseStock",
								    "StockId",
								    "Label",
								    "Active",
								    "Inconsistent");
			ToggleButtonProperties["StockId"].DependsOn (ToggleButtonProperties["UseStock"]);
			ToggleButtonProperties["Label"].DependsInverselyOn (ToggleButtonProperties["UseStock"]);

			groups = new PropertyGroup[] {
				ToggleButtonProperties,
				Button.ButtonExtraProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public ToggleButton (IStetic stetic) : this (stetic, new Gtk.ToggleButton ("Toggle")) {}

		public ToggleButton (IStetic stetic, Gtk.ToggleButton button) : base (stetic, button) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
