using Gtk;
using GLib;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class PropertyGrid : Stetic.Grid {

		Hashtable editors;
		Hashtable sensitives, invisibles;

		Stetic.Wrapper.Widget selection;
		Stetic.Wrapper.Container.ContainerChild packingSelection;

		public PropertyGrid (Project project)
		{
			project.Selected += Selected;
			Selected (null, null);
		}

		new void Clear ()
		{
			base.Clear ();
			if (selection != null) {
				selection.Notify -= Notified;
				selection = null;
			}
			editors = new Hashtable ();
			sensitives = new Hashtable ();
			invisibles = new Hashtable ();
		}

		PropertyDescriptor name;

		void AppendHeader ()
		{
			if (name == null)
				name = (PropertyDescriptor)Registry.LookupClass ("GtkWidget") ["Name"];
			AppendProperty (name, selection.Wrapped);

			Gtk.HBox box = new Gtk.HBox (false, 6);
			box.PackStart (new Gtk.Image (Registry.LookupClass (selection.Wrapped.GetType ()).Icon), false, false, 0);
			box.PackStart (new Gtk.Label (selection.Wrapped.GetType ().FullName), false, false, 0);
			box.ShowAll ();
			AppendPair ("Widget Class", box, null);
		}

		void AppendProperty (PropertyDescriptor prop, object obj)
		{
			PropertyEditor rep = new PropertyEditor (prop, obj);
			editors[prop.Name] = rep;
			if (prop.ParamSpec != null)
				editors[prop.ParamSpec.Name] = rep;
			rep.ShowAll ();

			AppendPair (prop.Label, rep, prop.Description);

			if (prop.HasDependencies)
				sensitives[prop] = obj;
			if (prop.HasVisibility)
				invisibles[prop] = obj;
		}

		void AppendCommand (CommandDescriptor cmd, object obj)
		{
			Gtk.Button button = new Gtk.Button (cmd.Label);
			button.Clicked += delegate (object o, EventArgs args) {
				cmd.Run (obj);
			};
			button.Show ();
			Append (button, cmd.Description);

			if (cmd.HasDependencies) {
				editors[cmd.Name] = button;
				sensitives[cmd] = obj;
			}
			if (cmd.HasVisibility) {
				editors[cmd.Name] = button;
				invisibles[cmd] = obj;
			}
		}

		void Notified (object wrapper, string propertyName)
		{
			PropertyEditor ed = editors[propertyName] as PropertyEditor;
			if (ed != null)
				ed.Update ();
			UpdateSensitivity ();
		}

		void UpdateSensitivity ()
		{
			foreach (ItemDescriptor item in sensitives.Keys) {
				Widget w = editors[item.Name] as Widget;
				if (w != null)
					w.Sensitive = item.EnabledFor (sensitives[item]);
			}
			foreach (ItemDescriptor item in invisibles.Keys) {
				Widget w = editors[item.Name] as Widget;
				if (w != null) {
					if (!item.VisibleFor (invisibles[item]))
						w.Hide ();
					else
						w.Show ();
				}
			}
		}

		void Selected (Stetic.Wrapper.Widget selection, ProjectNode node)
		{
			ClassDescriptor klass;

			Clear ();

			this.selection = selection;
			if (selection == null) {
				AppendLabel ("<i>No selection</i>");
				return;
			}

			selection.Notify += Notified;

			klass = Registry.LookupClass (selection.Wrapped.GetType ());

			AppendHeader ();
			AppendItemGroups (klass, selection.Wrapped);

			packingSelection = Stetic.Wrapper.Container.ChildWrapper (selection);
			if (packingSelection != null) {
				klass = Registry.LookupClass (packingSelection.Wrapped.GetType ());
				if (klass.ItemGroups.Count > 0) {
					AppendItemGroups (klass, packingSelection.Wrapped);
					packingSelection.Notify += Notified;
				}
			}

			UpdateSensitivity ();
		}

		void AppendItemGroups (ClassDescriptor klass, object obj)
		{
			bool first = true;
			foreach (ItemGroup igroup in klass.ItemGroups) {
				AppendGroup (igroup.Label, first);
				foreach (ItemDescriptor item in igroup.Items) {
					if (item is PropertyDescriptor)
						AppendProperty ((PropertyDescriptor)item, obj);
					else if (item is CommandDescriptor)
						AppendCommand ((CommandDescriptor)item, obj);
				}
				first = false;
			}
		}
	}
}
