using Gtk;
using GLib;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class PropertyGrid : Gtk.VBox {

		SizeGroup sgroup;
		Hashtable editors;
		ArrayList sensitives;

		protected object selection;
		protected Stetic.Wrapper.Object wrapper;

		public PropertyGrid () : base (false, 6)
		{
			BorderWidth = 2;

			sgroup = new SizeGroup (SizeGroupMode.Horizontal);
			NoSelection ();
		}

		protected void Clear ()
		{
			selection = null;
			if (wrapper != null) {
				wrapper.Notify -= Notified;
				wrapper = null;
			}
			foreach (Widget w in Children)
				Remove (w);
			editors = new Hashtable ();
			sensitives = new ArrayList ();
		}

		protected VBox AddGroup (string name)
		{
			Expander exp = new Expander ("<b>" + name + "</b>");
			exp.UseMarkup = true;

			VBox box = new VBox (true, 2);
			exp.Add (box);
			exp.ShowAll ();

			if (Children.Length == 0)
				exp.Expanded = true;

			PackStart (exp, false, false, 0);
			return box;
		}

		protected void AddToGroup (VBox group, PropertyDescriptor prop, object obj)
		{
			HBox box = new HBox (false, 6);

			Label label;

			label = new Label ("    ");
			box.PackStart (label, false, false, 0);

			label = new Label (prop.ParamSpec != null ? prop.ParamSpec.Nick : prop.Name);
			label.UseMarkup = true;
			label.Justify = Justification.Left;
			label.Xalign = 0;
			box.PackStart (label, true, true, 0);

			PropertyEditor rep = PropertyEditor.MakeEditor (prop, prop.ParamSpec, obj);
			if (rep != null) {
				editors[prop.Name] = rep;
				if (prop.ParamSpec != null)
					editors[prop.ParamSpec.Name] = rep;
				rep.ShowAll ();
				sgroup.AddWidget (rep);
				box.PackStart (rep, false, false, 0);
			}

			box.ShowAll ();
			group.PackStart (box, false, false, 0);

			if (prop.Dependencies.Count > 0 || prop.InverseDependencies.Count > 0)
				sensitives.Add (prop);
		}

		public void NoSelection ()
		{
			Clear ();

			Label label = new Label ("<i>No selection</i>");
			label.UseMarkup = true;
			label.Justify = Justification.Left;
			label.Xalign = 0;
			label.Show ();
			PackStart (label, true, true, 0);
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
			VBox group = AddGroup ("Properties");

			foreach (PropertyInfo info in obj.GetType().GetProperties (BindingFlags.Instance | BindingFlags.Public)) {
				ParamSpec pspec = LookupParamSpec (pspecObj, info);
				if (pspec != null)
					AddToGroup (group, new PropertyDescriptor (obj.GetType(), info.Name), obj);
			}
		}

		protected void AddObjectWrapperProperties (object obj, PropertyGroup[] groups)
		{
			foreach (PropertyGroup pgroup in groups) {
				VBox group = AddGroup (pgroup.Name);
				foreach (PropertyDescriptor prop in pgroup.Properties)
					AddToGroup (group, prop, obj);
			}
		}
	}

#if NO
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
			wrapper = Stetic.WidgetFactory.GetWrapper (parent) as IContainerWrapper;
			if (wrapper != null)
				AddObjectWrapperProperties (cc, ((IContainerWrapper)wrapper).ChildPropertyGroups);
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
#endif
}
