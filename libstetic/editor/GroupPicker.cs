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
		ArrayList values;
		EventInfo groupChanged;

		public GroupPicker (PropertyInfo info) : base (false, 0)
		{
			BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
			Type owner = info.DeclaringType;

			FieldInfo groupInfo = owner.GetField (info.Name + "List", flags);
			if (groupInfo == null || groupInfo.FieldType != typeof (ArrayList))
				throw new ArgumentException ("No 'static ArrayList " + info.Name + "List' property on " + owner.FullName);

			values = groupInfo.GetValue (null) as ArrayList;

			groupChanged = owner.GetEvent (info.Name + "ListChanged", flags);
			if (groupChanged != null)
				groupChanged.AddEventHandler (null, new ListChangedDelegate (ListChanged));

			ListChanged ();
		}

		protected override void OnDestroyed ()
		{
			groupChanged.RemoveEventHandler (null, new ListChangedDelegate (ListChanged));
		}

		public delegate void ListChangedDelegate ();

		void ListChanged ()
		{
			if (combo != null) {
				combo.Changed -= combo_Changed;
				Remove (combo);
			}

			combo = Gtk.ComboBox.NewText ();
			combo.Changed += combo_Changed;
			combo.Show ();
			PackStart (combo, true, true, 0);

			for (int i = 0; i < values.Count; i++) {
				combo.AppendText (values[i] as string);
				if (values[i] == groupname)
					combo.Active = i;
			}

			combo.AppendText ("Rename Group...");
			combo.AppendText ("New Group...");
		}

		int group;
		string groupname;
		public int Group {
			get {
				return group;
			}
			set {
				if (value < 0 || value >= values.Count)
					return;
				group = combo.Active = value;
				groupname = values[value] as string;
			}
		}

		public event EventHandler Changed;

		void combo_Changed (object o, EventArgs args)
		{
			if (combo.Active >= values.Count) {
				doDialog ();
				return;
			}

			group = combo.Active;
			groupname = values[group] as string;
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}

		void doDialog ()
		{
			bool rename = combo.Active == values.Count;
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
				entry.Text = groupname;
			hbox.PackStart (entry, true, true, 0);
			dialog.VBox.PackStart (hbox, false, false, 0);

			dialog.ShowAll ();
			// Have to set this *after* ShowAll
			dialog.ActionArea.BorderWidth = 0;
			Gtk.ResponseType response = (Gtk.ResponseType)dialog.Run ();
			if (response == Gtk.ResponseType.Cancel || entry.Text.Length == 0) {
				dialog.Destroy ();
				combo.Active = group;
				return;
			}

			groupname = entry.Text;
			dialog.Destroy ();

			if (rename)
				values[group] = groupname;
			else
				values.Add (groupname);
			ListChanged ();
		}
	}
}
