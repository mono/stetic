using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Radio Button", "radiobutton.png", ObjectWrapperType.Widget)]
	public class RadioButton : Button {

		public static new Type WrappedType = typeof (Gtk.RadioButton);

		internal static new void Register (Type type)
		{
			AddItemGroup (type, "Radio Button Properties",
				      "Label",
				      "Group",
				      "Active",
				      "Inconsistent",
				      "DrawIndicator");
		}

		static RadioGroupManager GroupManager = new RadioGroupManager (WrappedType);

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);

			Gtk.RadioButton radiobutton = (Gtk.RadioButton)Wrapped;
			if (!initialized) {
				radiobutton.Label = radiobutton.Name;
				Group = GroupManager.LastGroup;
			} else if (radiobutton.Group == null)
				Group = radiobutton.Name;
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

		public override bool HExpandable { get { return true; } }
	}
}
