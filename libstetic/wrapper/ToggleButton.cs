using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Toggle Button", "togglebutton.png", ObjectWrapperType.Widget)]
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

			groups = new ItemGroup[] {
				ToggleButtonProperties,
				Button.ButtonExtraProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public ToggleButton (IStetic stetic) : this (stetic, new Gtk.ToggleButton ()) {}

		public ToggleButton (IStetic stetic, Gtk.ToggleButton button) : base (stetic, button)
		{
			button.Label = button.Name;
		}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }
	}
}
