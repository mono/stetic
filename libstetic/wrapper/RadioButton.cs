using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Radio Button", "radiobutton.png", ObjectWrapperType.Widget)]
	public class RadioButton : Button {

		static ArrayList GroupList = new ArrayList ();
		static event Stetic.Editor.GroupPicker.ListChangedDelegate GroupListChanged;
		static ArrayList GroupLeaders = new ArrayList ();

		public static new Type WrappedType = typeof (Gtk.RadioButton);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Radio Button Properties",
				      "Label",
				      "Group",
				      "Active",
				      "Inconsistent",
				      "DrawIndicator");
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				Gtk.RadioButton radiobutton = (Gtk.RadioButton)Wrapped;

				radiobutton.Label = radiobutton.Name;

				if (GroupList.Count == 0) {
					GroupList.Add ("radiogroup1");
					GroupLeaders.Add (radiobutton);
				}
				Group = GroupList.Count - 1;
			}
		}

		string group;

		protected override void GladeImport (string className, string id, Hashtable props)
		{
			group = GladeUtils.ExtractProperty ("group", props);
			base.GladeImport (className, id, props);

			if (group != null)
				stetic.GladeImportComplete += SetGroup;
			else {
				GroupList.Add (((Gtk.Widget)Wrapped).Name);
				GroupLeaders.Add (Wrapped);
			}
		}

		void SetGroup ()
		{
			Gtk.Widget leader = stetic.LookupWidgetById (group);
			if (leader != null) {
				int index = GroupLeaders.IndexOf (leader);
				if (index != -1)
					Group = index;
			}
			stetic.GladeImportComplete -= SetGroup;
		}

		[Editor (typeof (Stetic.Editor.GroupPicker))]
		[Description ("Group", "The name of the radio button group that this button belongs to")]
		public int Group {
			get {
				// We can't store this in an instance variable
				// because groups may be removed from the list. So
				// just figure it out from scratch each time.

				Gtk.RadioButton radio = (Gtk.RadioButton)Wrapped;
				GLib.SList group = radio.Group;

				for (int i = 0; i < GroupLeaders.Count; i++) {
					if ((Gtk.RadioButton)GroupLeaders[i] == radio)
						return i;
					foreach (object member in group) {
						if (member == GroupLeaders[i])
							return i;
					}
				}

				// can't happen?
				return -1;
			}
			set {
				Gtk.RadioButton radio = (Gtk.RadioButton)Wrapped;
				GLib.SList group;
				bool killGroup = false;

				// shouldn't happen...
				if (value < 0 || value > GroupLeaders.Count)
					return;

				// check if we are currently the leader of a group
				int oldGroup = GroupLeaders.IndexOf (radio);
				if (oldGroup == value)
					return; // no change
				else if (oldGroup != -1) {
					group = radio.Group;
					killGroup = true;
					foreach (Gtk.RadioButton member in group) {
						if (member != radio) {
							GroupLeaders[oldGroup] = member;
							killGroup = false;
						}
					}
				}

				if (value < GroupLeaders.Count) {
					// Add us to an existing group
					group = ((Gtk.RadioButton)GroupLeaders[value]).Group;
					bool found = false;
					foreach (Gtk.RadioButton member in group) {
						if (member == radio) {
							found = true;
							break;
						}
					}
					if (!found)
						radio.Group = group;
				} else {
					// Create a new group for just us
					GroupLeaders.Add (radio);
					radio.Group = new GLib.SList(IntPtr.Zero);
				}

				EmitNotify ("Group");
				if (killGroup) {
					GroupLeaders.RemoveAt (oldGroup);
					GroupList.RemoveAt (oldGroup);
					if (GroupListChanged != null)
						GroupListChanged ();
				}
			}
		}
	}
}
