using System;
using System.Collections;

namespace Stetic.Wrapper {

	public class RadioButton : CheckButton {

		static RadioGroupManager GroupManager = new RadioGroupManager (typeof (Gtk.RadioButton));

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
