using System;
using System.Collections;
using System.Xml;

namespace Stetic.Wrapper {

	public class RadioButton : CheckButton {

		static RadioGroupManager GroupManager = new RadioGroupManager (typeof (Gtk.RadioButton));

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);

			Gtk.RadioButton radiobutton = (Gtk.RadioButton)Wrapped;
			if (!initialized)
				Group = GroupManager.LastGroup;
			else if (radiobutton.Group == null)
				Group = radiobutton.Name;
		}

		public override void GladeImport (XmlElement elem)
		{
			string group = (string)GladeUtils.ExtractProperty (elem, "group", "");
			bool active = (bool)GladeUtils.ExtractProperty (elem, "active", false);
			base.GladeImport (elem);

			if (group != "")
				Group = group;
			else
				Group = Wrapped.Name;
			if (active)
				((Gtk.RadioButton)Wrapped).Active = true;
		}

		public override XmlElement GladeExport (XmlDocument doc)
		{
			XmlElement elem = base.GladeExport (doc);
			string group = GroupManager.GladeGroupName (Wrapped);
			if (group != Wrapped.Name)
				GladeUtils.SetProperty (elem, "group", group);
			return elem;
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
