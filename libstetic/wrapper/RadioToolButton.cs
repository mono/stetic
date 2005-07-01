using System;
using System.Collections;

namespace Stetic.Wrapper {

	public class RadioToolButton : ToggleToolButton {

		public static new Gtk.ToolButton CreateInstance ()
		{
			return new Gtk.RadioToolButton (new GLib.SList (IntPtr.Zero), Gtk.Stock.SortAscending);
		}

		static RadioGroupManager GroupManager = new RadioGroupManager (typeof (Gtk.RadioToolButton));

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
			string active = GladeUtils.ExtractProperty ("active", props);
			base.GladeImport (className, id, props);

			if (group != null)
				Group = group;
			else
				Group = Wrapped.Name;
			if (active == "True")
				((Gtk.RadioToolButton)Wrapped).Active = true;
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
