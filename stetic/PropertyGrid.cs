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

		protected Stetic.Grid.Group AddGroup (string name)
		{
			return AddGroup (name, groups.Count == 0);
		}

		protected void AddToGroup (Stetic.Grid.Group group, PropertyDescriptor prop, object obj)
		{
			string label = prop.ParamSpec != null ? prop.ParamSpec.Nick : prop.Name;

			PropertyEditor rep = PropertyEditor.MakeEditor (prop, prop.ParamSpec, obj);
			editors[prop.Name] = rep;
			if (prop.ParamSpec != null)
				editors[prop.ParamSpec.Name] = rep;
			rep.ShowAll ();

			group.Add (label, rep);

			if (prop.Dependencies.Count > 0 || prop.InverseDependencies.Count > 0)
				sensitives.Add (prop);
		}

		public void NoSelection ()
		{
			Clear ();
			AddGroup ("<i>No selection</i>", false);
		}

		void Notified (Stetic.Wrapper.Object wrapper, string propertyName)
		{
			PropertyEditor ed = editors[propertyName] as PropertyEditor;
			if (ed != null)
				ed.Update (wrapper, EventArgs.Empty);
			UpdateSensitivity ();
		}

		protected void UpdateSensitivity ()
		{
			foreach (PropertyDescriptor prop in sensitives) {
				Widget w = editors[prop.Name] as Widget;
				if (w == null)
					continue;

				foreach (PropertyDescriptor dep in prop.Dependencies) {
					if (!(bool)dep.GetValue (wrapper)) {
						w.Sensitive = false;
						goto next;
					}
				}
				foreach (PropertyDescriptor dep in prop.InverseDependencies) {
					if ((bool)dep.GetValue (wrapper)) {
						w.Sensitive = false;
						goto next;
					}
				}
				w.Sensitive = true;

			next:
				;
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

			if (wrapper != null) {
				wrapper.Notify += Notified;
				AddObjectWrapperProperties (w, wrapper.PropertyGroups);
			} else
				AddParamSpecProperties (w, w);

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

		protected void AddParamSpecProperties (object obj, GLib.Object pspecObj)
		{
			Stetic.Grid.Group group = AddGroup ("Properties");

			foreach (PropertyInfo info in obj.GetType().GetProperties (BindingFlags.Instance | BindingFlags.Public)) {
				ParamSpec pspec = LookupParamSpec (pspecObj, info);
				if (pspec != null)
					AddToGroup (group, new PropertyDescriptor (obj.GetType(), info.Name), obj);
			}
		}

		protected void AddObjectWrapperProperties (object obj, PropertyGroup[] groups)
		{
			foreach (PropertyGroup pgroup in groups) {
				Stetic.Grid.Group group = AddGroup (pgroup.Name);
				foreach (PropertyDescriptor prop in pgroup.Properties)
					AddToGroup (group, prop, obj);
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
				AddObjectWrapperProperties (cc, ((Stetic.Wrapper.Container)wrapper).ChildPropertyGroups);
			else
				AddParamSpecProperties (cc, parent);

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
