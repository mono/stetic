using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Toolbar Radio Button", "radiobutton.png", ObjectWrapperType.ToolbarItem)]
	public class RadioToolButton : ToggleToolButton {

		public static new Type WrappedType = typeof (Gtk.RadioToolButton);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Toolbar Toggle Button Properties",
				      "Icon",
				      "Label",
				      "UseUnderline",
				      "Group",
				      "Active");
		}

		public static new Gtk.ToolButton CreateInstance ()
		{
			return new Gtk.RadioToolButton (new GLib.SList (IntPtr.Zero), Gtk.Stock.SortAscending);
		}

		static RadioGroupManager GroupManager = new RadioGroupManager (WrappedType);

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);

			Gtk.RadioToolButton radio = (Gtk.RadioToolButton)Wrapped;
			if (!initialized)
				Group = GroupManager.LastGroup;
			else if (radio.Group == null)
				Group = radio.Name;
		}

		protected override void GladeImport (string className, string id, Hashtable props)
		{
			string group = GladeUtils.ExtractProperty ("group", props);
			base.GladeImport (className, id, props);

			if (group != null)
				Group = group;
			else
				Group = Wrapped.Name;
		}

		[Editor (typeof (Stetic.Editor.GroupPicker))]
		[Description ("Group", "The name of the radio button group that this button belongs to")]
		public string Group {
			get {
				return GroupManager[Wrapped];
			}
			set {
				GroupManager[Wrapped] = value;
			}
		}
	}
}
