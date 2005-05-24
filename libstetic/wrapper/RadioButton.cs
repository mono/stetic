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
			string active = GladeUtils.ExtractProperty ("active", props);
			base.GladeImport (className, id, props);

			if (group != null)
				Group = group;
			else
				Group = Wrapped.Name;
			if (active == "True")
				((Gtk.RadioButton)Wrapped).Active = true;
		}

		public override void GladeExport (out string className, out string id, out Hashtable props)
		{
			base.GladeExport (out className, out id, out props);
			string group = GroupManager.GladeGroupName (Wrapped);
			if (group != id)
				props["group"] = group;
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
