using Gtk;
using GLib;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class PropertyGrid : Stetic.Grid {

		Hashtable editors;
		ArrayList sensitives;

		ObjectWrapper selection;

		public PropertyGrid ()
		{
			NoSelection ();
		}

		protected new void Clear ()
		{
			base.Clear ();
			if (selection != null) {
				selection.Notify -= Notified;
				selection = null;
			}
			editors = new Hashtable ();
			sensitives = new ArrayList ();
		}

		protected void AppendProperty (PropertyDescriptor prop)
		{
			PropertyEditor rep = PropertyEditor.MakeEditor (prop, prop.ParamSpec, selection);
			editors[prop.Name] = rep;
			if (prop.ParamSpec != null)
				editors[prop.ParamSpec.Name] = rep;
			rep.ShowAll ();

			AppendPair (prop.Label, rep, prop.Description);

			if (prop.HasDependencies)
				sensitives.Add (prop);
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

		protected void AppendCommand (CommandDescriptor cmd)
		{
			Gtk.Button button = new Gtk.Button (cmd.Label);
			button.Clicked += new Stupid69614Workaround (cmd, selection).Activate;
			button.Show ();
			Append (button, cmd.Description);

			if (cmd.HasDependencies) {
				editors[cmd.Name] = button;
				sensitives.Add (cmd);
			}
		}

		void Notified (object wrapper, string propertyName)
		{
			PropertyEditor ed = editors[propertyName] as PropertyEditor;
			if (ed != null)
				ed.Update (wrapper, EventArgs.Empty);
			UpdateSensitivity ();
		}

		protected void UpdateSensitivity ()
		{
			foreach (ItemDescriptor item in sensitives) {
				Widget w = editors[item.Name] as Widget;
				if (w != null)
					w.Sensitive = item.EnabledFor (selection);
			}
		}

		public virtual ObjectWrapper GetWrapperForSite (IWidgetSite site)
		{
			return Stetic.ObjectWrapper.Lookup (site.Contents);
		}

		public void Select (IWidgetSite site)
		{
			Clear ();

			selection = GetWrapperForSite (site);
			if (selection == null)
				return;
			selection.Notify += Notified;

			if (selection is Stetic.Wrapper.Widget)
				AppendProperty (new PropertyDescriptor (typeof (Gtk.Widget), "Name"));

			bool first = true;
			foreach (ItemGroup igroup in selection.ItemGroups) {
				AppendGroup (igroup.Name, first);
				foreach (ItemDescriptor item in igroup.Items) {
					if (item is PropertyDescriptor)
						AppendProperty ((PropertyDescriptor)item);
					else if (item is CommandDescriptor)
						AppendCommand ((CommandDescriptor)item);
				}
				first = false;
			}
			UpdateSensitivity ();
		}

		public void NoSelection ()
		{
			Clear ();
			AppendLabel ("<i>No selection</i>");
		}
	}

	public class ChildPropertyGrid : PropertyGrid {

		public override ObjectWrapper GetWrapperForSite (IWidgetSite site)
		{
			return Stetic.Wrapper.Container.ChildWrapper (site);
		}
	}
}
