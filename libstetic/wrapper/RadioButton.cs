using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Radio Button", "radiobutton.png", ObjectWrapperType.Widget)]
	public class RadioButton : ToggleButton {

		public static ItemGroup RadioButtonProperties;
		public static ItemGroup RadioButtonExtraProperties;

		static ArrayList GroupList;
		static event Stetic.Editor.GroupPicker.ListChangedDelegate GroupListChanged;
		static ArrayList GroupLeaders;

		static RadioButton () {
			RadioButtonProperties = new ItemGroup ("Radio Button Properties",
							       typeof (Stetic.Wrapper.RadioButton),
							       typeof (Gtk.RadioButton),
							       "Label",
							       "Group",
							       "Active",
							       "Inconsistent",
							       "DrawIndicator");

			groups = new ItemGroup[] {
				RadioButtonProperties, Button.ButtonExtraProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			GroupList = new ArrayList ();
			GroupLeaders = new ArrayList ();
		}

		public RadioButton (IStetic stetic) : this (stetic, new Gtk.RadioButton ("")) {}

		public RadioButton (IStetic stetic, Gtk.RadioButton radiobutton) : base (stetic, radiobutton)
		{
			radiobutton.Label = radiobutton.Name;

			if (GroupList.Count == 0) {
				GroupList.Add ("radiogroup1");
				GroupLeaders.Add (radiobutton);
			}
			Group = GroupList.Count - 1;
		}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }

		[Editor (typeof (Stetic.Editor.GroupPicker))]
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
