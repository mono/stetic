using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Check Box", "checkbutton.png", ObjectWrapperType.Widget)]
	public class CheckButton : Button {

		public static new Type WrappedType = typeof (Gtk.CheckButton);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Check Box Properties",
				      "Label",
				      "Active",
				      "Inconsistent",
				      "DrawIndicator");
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				Gtk.CheckButton checkbutton = (Gtk.CheckButton)obj;
				checkbutton.Label = checkbutton.Name;
			}
		}

		public override bool HExpandable { get { return true; } }
	}
}
