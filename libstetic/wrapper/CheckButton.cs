using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Check Box", "checkbutton.png", typeof (Gtk.CheckButton), ObjectWrapperType.Widget)]
	public class CheckButton : ToggleButton {

		public static ItemGroup CheckButtonProperties;

		static CheckButton () {
			CheckButtonProperties = new ItemGroup ("Check Box Properties",
							       typeof (Gtk.CheckButton),
							       "Label",
							       "Active",
							       "Inconsistent",
							       "DrawIndicator");
			RegisterWrapper (typeof (Stetic.Wrapper.CheckButton),
					 CheckButtonProperties,
					 Button.ButtonExtraProperties,
					 Widget.CommonWidgetProperties);
		}

		public CheckButton (IStetic stetic) : this (stetic, new Gtk.CheckButton (), false) {}


		public CheckButton (IStetic stetic, Gtk.CheckButton checkbutton, bool initialized) : base (stetic, checkbutton, initialized)
		{
			if (!initialized) {
				checkbutton.Label = checkbutton.Name;
			}
		}
	}
}