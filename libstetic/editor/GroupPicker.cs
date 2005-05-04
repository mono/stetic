using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic.Editor {

	[PropertyEditor ("Group", "Changed")]
	class GroupPicker : Gtk.HBox {

		Gtk.ComboBox combo;
		RadioGroupManager manager;
		ArrayList values;
		string group;

		public GroupPicker (PropertyInfo info) : base (false, 0)
		{
			BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			Type owner = info.DeclaringType;

			FieldInfo managerInfo = owner.GetField (info.Name + "Manager", flags);
			if (managerInfo == null || managerInfo.FieldType != typeof (Stetic.RadioGroupManager))
				throw new ArgumentException ("No 'static RadioGroupManager " + info.Name + "Manager' property on " + owner.FullName);

			manager = managerInfo.GetValue (null) as RadioGroupManager;
			manager.GroupsChanged += GroupsChanged;
			GroupsChanged ();
		}

		protected override void OnDestroyed ()
		{
			manager.GroupsChanged -= GroupsChanged;
		}

		void GroupsChanged ()
		{
			if (combo != null) {
				combo.Changed -= combo_Changed;
				Remove (combo);
			}

			combo = Gtk.ComboBox.NewText ();
			combo.Changed += combo_Changed;
			combo.RowSeparatorFunc = RowSeparatorFunc;
			combo.Show ();
			PackStart (combo, true, true, 0);

			values = new ArrayList ();
			int i = 0;
			foreach (string name in manager.GroupNames) {
				values.Add (name);
				combo.AppendText (name);
				if (name == group)
					combo.Active = i;
				i++;
			}

			combo.AppendText ("");

			combo.AppendText ("Rename Group...");
			combo.AppendText ("New Group...");
		}

		bool RowSeparatorFunc (Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			GLib.Value val = new GLib.Value ();
			model.GetValue (iter, 0, ref val);
			bool sep = ((string)val) == "";
			val.Dispose ();
			return sep;
		}

		public string Group {
			get {
				return group;
			}
			set {
				int index = values.IndexOf (value);
				if (index != -1) {
					combo.Active = index;
					group = values[index] as string;
				}
			}
		}

		public event EventHandler Changed;

		void combo_Changed (object o, EventArgs args)
		{
			if (combo.Active >= values.Count) {
				doDialog ();
				return;
			}

			group = values[combo.Active] as string;
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		void doDialog ()
		{
			bool rename = combo.Active == values.Count + 1;
			Gtk.Dialog dialog = new Gtk.Dialog (
				rename ? "Rename Group" : "New Group",
				combo.Toplevel as Gtk.Window,
				Gtk.DialogFlags.Modal | Gtk.DialogFlags.NoSeparator, 
				Gtk.Stock.Cancel, Gtk.ResponseType.Cancel,
				Gtk.Stock.Ok, Gtk.ResponseType.Ok);
			dialog.DefaultResponse = Gtk.ResponseType.Ok;
			dialog.HasSeparator = false;
			dialog.BorderWidth = 12;
			dialog.VBox.Spacing = 18;
			dialog.VBox.BorderWidth = 0;

			Gtk.HBox hbox = new Gtk.HBox (false, 12);
			Gtk.Label label = new Gtk.Label (rename ? "_New name:" : "_Name:");
			Gtk.Entry entry = new Gtk.Entry ();
			label.MnemonicWidget = entry;
			hbox.PackStart (label, false, false, 0);
			entry.ActivatesDefault = true;
			if (rename)
				entry.Text = group;
			hbox.PackStart (entry, true, true, 0);
			dialog.VBox.PackStart (hbox, false, false, 0);

			dialog.ShowAll ();
			// Have to set this *after* ShowAll
			dialog.ActionArea.BorderWidth = 0;
			Gtk.ResponseType response = (Gtk.ResponseType)dialog.Run ();
			if (response == Gtk.ResponseType.Cancel || entry.Text.Length == 0) {
				dialog.Destroy ();
				Group = group; // reset combo.Active
				return;
			}

			string oldname = group;
			group = entry.Text;
			dialog.Destroy ();

			// FIXME: check that the new name doesn't already exist

			// This will trigger a GroupsChanged, which will eventually
			// update combo.Active
			if (rename)
				manager.Rename (oldname, group);
			else
				manager.Add (group);
		}
	}
}
