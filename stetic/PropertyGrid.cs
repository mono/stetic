using Gtk;
using GLib;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class PropertyGrid : Stetic.Grid {

		Hashtable editors;
		Hashtable sensitives;

		ObjectWrapper selection, packingSelection;

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
		}

		void AppendProperty (PropertyDescriptor prop, ObjectWrapper wrapper)
		{
			if (!prop.VisibleFor (wrapper))
				return;

			PropertyEditor rep = PropertyEditor.MakeEditor (prop, wrapper);
			editors[prop.Name] = rep;
			if (prop.ParamSpec != null)
				editors[prop.ParamSpec.Name] = rep;
			rep.ShowAll ();

			AppendPair (prop.Label, rep, prop.Description);

			if (prop.HasDependencies)
				sensitives[prop] = wrapper;
		}

		private class Stupid69614Workaround {
			CommandDescriptor cmd;
			ObjectWrapper wrapper;

			public Stupid69614Workaround (CommandDescriptor cmd, ObjectWrapper wrapper)
			{
				this.cmd = cmd;
				this.wrapper = wrapper;
			}

			public void Activate (object o, EventArgs args) {
				cmd.Run (wrapper);
			}
		}

		void AppendCommand (CommandDescriptor cmd, ObjectWrapper wrapper)
		{
			Gtk.Button button = new Gtk.Button (cmd.Label);
			button.Clicked += new Stupid69614Workaround (cmd, wrapper).Activate;
			button.Show ();
			Append (button, cmd.Description);

			if (cmd.HasDependencies) {
				editors[cmd.Name] = button;
				sensitives[cmd] = wrapper;
			}
		}

		void Notified (object wrapper, string propertyName)
		{
			PropertyEditor ed = editors[propertyName] as PropertyEditor;
			if (ed != null)
				ed.Update (wrapper, EventArgs.Empty);
			UpdateSensitivity ();
		}

		void UpdateSensitivity ()
		{
			foreach (ItemDescriptor item in sensitives.Keys) {
				Widget w = editors[item.Name] as Widget;
				if (w != null)
					w.Sensitive = item.EnabledFor (sensitives[item] as ObjectWrapper);
			}
		}

		void Selected (IWidgetSite site, ProjectNode node)
		{
			Clear ();

			if (site == null) {
				AppendLabel ("<i>No selection</i>");
				return;
			}

			selection = Stetic.ObjectWrapper.Lookup (site.Contents);
			if (selection == null)
				return;
			selection.Notify += Notified;

			if (selection is Stetic.Wrapper.Widget)
				AppendProperty (new PropertyDescriptor (typeof (Stetic.Wrapper.Widget), typeof (Gtk.Widget), "Name"), selection);
			AppendWrapperGroups (selection);

			packingSelection = Stetic.Wrapper.Container.ChildWrapper (site);
			if (packingSelection != null && packingSelection.ItemGroups.Count > 0) {
				AppendWrapperGroups (packingSelection);
				packingSelection.Notify += Notified;
			}

			UpdateSensitivity ();
		}

		void AppendWrapperGroups (ObjectWrapper wrapper)
		{
			bool first = true;
			foreach (ItemGroup igroup in wrapper.ItemGroups) {
				AppendGroup (igroup.Name, first);
				foreach (ItemDescriptor item in igroup.Items) {
					if (item is PropertyDescriptor)
						AppendProperty ((PropertyDescriptor)item, wrapper);
					else if (item is CommandDescriptor)
						AppendCommand ((CommandDescriptor)item, wrapper);
				}
				first = false;
			}
		}
	}
}
