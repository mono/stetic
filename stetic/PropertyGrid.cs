using Gtk;
using GLib;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class PropertyGrid : Stetic.Grid {

		Hashtable editors;
		ArrayList sensitives;

		protected object selection;
		protected Stetic.Wrapper.Object wrapper;

		public PropertyGrid ()
		{
			NoSelection ();
		}

		protected new void Clear ()
		{
			base.Clear ();
			selection = null;
			if (wrapper != null) {
				wrapper.Notify -= Notified;
				wrapper = null;
			}
			editors = new Hashtable ();
			sensitives = new ArrayList ();
		}

		protected void AppendProperty (PropertyDescriptor prop, object obj)
		{
			string label = prop.ParamSpec != null ? prop.ParamSpec.Nick : prop.Name;

			PropertyEditor rep = PropertyEditor.MakeEditor (prop, prop.ParamSpec, obj);
			editors[prop.Name] = rep;
			if (prop.ParamSpec != null)
				editors[prop.ParamSpec.Name] = rep;
			rep.ShowAll ();

			AppendPair (label, rep);

			if (prop.HasDependencies)
				sensitives.Add (prop);
		}

		private class Stupid69614Workaround {
			CommandDescriptor cmd;
			object obj;

			public Stupid69614Workaround (CommandDescriptor cmd, object obj)
			{
				this.cmd = cmd;
				this.obj = obj;
			}

			public void Activate (object o, EventArgs args) {
				cmd.Run (obj);
			}
		}

		protected void AppendCommand (CommandDescriptor cmd, object obj)
		{
			Gtk.Button button = new Gtk.Button (cmd.Label);
			button.Clicked += new Stupid69614Workaround (cmd, wrapper).Activate;
			button.Show ();
			Append (button);

			if (cmd.HasDependencies) {
				editors[cmd.Name] = button;
				sensitives.Add (cmd);
			}
		}

		public void NoSelection ()
		{
			Clear ();
			AppendLabel ("<i>No selection</i>");
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
					w.Sensitive = item.EnabledFor (wrapper);
			}
		}

		protected void UpdateSensitivity (object obj, EventArgs args)
		{
			UpdateSensitivity ();
		}

		public void Select (IWidgetSite site)
		{
			Clear ();

			Widget w = site.Contents;
			if (w == null)
				return;

			selection = w;
			wrapper = Stetic.Wrapper.Object.Lookup (w);

			AppendProperty (new PropertyDescriptor (typeof (Gtk.Widget), "Name"), w); 

			if (wrapper != null) {
				wrapper.Notify += Notified;
				AddObjectWrapperItems (w, wrapper.ItemGroups);
			} else
				AddParamSpecItems (w, w);

			UpdateSensitivity ();
		}

		public virtual ParamSpec LookupParamSpec (object obj, PropertyInfo info)
		{
			foreach (object attr in info.GetCustomAttributes (typeof (GLib.PropertyAttribute), false)) {
				PropertyAttribute pattr = (PropertyAttribute)attr;
				return ParamSpec.LookupObjectProperty (obj.GetType(), pattr.Name);
			}
			return null;
		}

		protected void AddParamSpecItems (object obj, GLib.Object pspecObj)
		{
			AppendGroup ("Properties", true);

			foreach (PropertyInfo info in obj.GetType().GetProperties (BindingFlags.Instance | BindingFlags.Public)) {
				ParamSpec pspec = LookupParamSpec (pspecObj, info);
				if (pspec != null)
					AppendProperty (new PropertyDescriptor (obj.GetType(), info.Name), obj);
			}
		}

		protected void AddObjectWrapperItems (object obj, ItemGroup[] groups)
		{
			bool first = true;

			foreach (ItemGroup igroup in groups) {
				AppendGroup (igroup.Name, first);
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

	public class ChildPropertyGrid : PropertyGrid {

		public new void Select (IWidgetSite site)
		{
			Clear ();

			if (!(site is Widget) || (site.ParentSite == null))
				return;

			Widget w = site.Contents;
			if (w == null)
				return;

			Container parent = site.ParentSite.Contents as Container;
			if (parent == null)
				return;

			ContainerChild cc = parent[(Widget)site];
			selection = cc;
			wrapper = Stetic.Wrapper.Container.Lookup (parent) as Stetic.Wrapper.Container;
			if (wrapper != null)
				AddObjectWrapperItems (cc, ((Stetic.Wrapper.Container)wrapper).ChildItemGroups);
			else
				AddParamSpecItems (cc, parent);

		       UpdateSensitivity ();
		}

		public override ParamSpec LookupParamSpec (object obj, PropertyInfo info)
		{
			foreach (object attr in info.GetCustomAttributes (typeof (Gtk.ChildPropertyAttribute), false)) {
				ChildPropertyAttribute cpattr = (ChildPropertyAttribute)attr;
				return ParamSpec.LookupChildProperty (obj.GetType(), cpattr.Name);
			}
			return null;
		}
	}
}
