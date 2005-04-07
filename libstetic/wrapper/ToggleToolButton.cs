using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Toolbar Toggle Button", "checkbutton.png", ObjectWrapperType.ToolbarItem)]
	public class ToggleToolButton : ToolButton {

		public static new Type WrappedType = typeof (Gtk.ToggleToolButton);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Toolbar Toggle Button Properties",
				      "Icon",
				      "Label",
				      "UseUnderline",
				      "Active");
		}

		public static new Gtk.ToolButton CreateInstance ()
		{
			return new Gtk.ToggleToolButton (Gtk.Stock.Bold);
		}

		// FIXME. Not needed in 2.6
		[GladeProperty (Name = "active", Proxy = "GladeActive")]
		public bool Active {
			get {
				return ((Gtk.ToggleToolButton)Wrapped).Active;
			}
			set {
				((Gtk.ToggleToolButton)Wrapped).Active = value;
			}
		}

		public string GladeActive {
			get {
				return Active ? "True" : "False";
			}
			set {
				Active = (value == "True");
			}
		}
	}
}
