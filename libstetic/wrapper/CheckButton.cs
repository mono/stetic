using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Check Box", "checkbutton.png", ObjectWrapperType.Widget)]
	public class CheckButton : ToggleButton {

		public static ItemGroup CheckButtonProperties;

		static CheckButton () {
			CheckButtonProperties = new ItemGroup ("Check Box Properties",
							       typeof (Gtk.CheckButton),
							       "Label",
							       "Active",
							       "Inconsistent",
							       "DrawIndicator");

			groups = new ItemGroup[] {
				CheckButtonProperties, Button.ButtonExtraProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public CheckButton (IStetic stetic) : this (stetic, new Gtk.CheckButton ()) {}

		public CheckButton (IStetic stetic, Gtk.CheckButton checkbutton) : base (stetic, checkbutton)
		{
			checkbutton.Label = checkbutton.Name;
		}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }
	}
}
