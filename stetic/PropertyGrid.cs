using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class PropertyGrid : Gtk.VBox {

		SizeGroup sgroup;
		Hashtable editors;

		GLib.Object selection;
		IntPtr notifyId;

		public PropertyGrid () : base (false, 6)
		{
			BorderWidth = 2;

			sgroup = new SizeGroup (SizeGroupMode.Horizontal);
			NoSelection ();
		}

		protected void Clear ()
		{
			if (selection != null) {
				Notify.Remove (selection, notifyId);
				selection = null;
				notifyId = IntPtr.Zero;
			}
			foreach (Widget w in Children)
				Remove (w);
			editors = new Hashtable ();
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
				else if (prop.EventInfo != null)
					prop.EventInfo.AddEventHandler (selection, new EventHandler (rep.Update));
				rep.ShowAll ();
				sgroup.AddWidget (rep);
				box.PackStart (rep, false, false, 0);
			}

			box.ShowAll ();
			group.PackStart (box, false, false, 0);
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

		void SensitivityChanged (string prop, bool sensitivity)
		{
			Widget w = editors[prop] as Widget;
			if (w != null)
				w.Sensitive = sensitivity;
		}

		void Notified (ParamSpec pspec)
		{
			PropertyEditor ed = editors[pspec.Name] as PropertyEditor;
			if (ed != null)
				ed.Update (selection, EventArgs.Empty);
		}

		public void Select (WidgetBox wbox)
		{
			Clear ();

			Widget w = wbox.Child;
			if (w == null)
				return;

			selection = w;
			notifyId = Notify.Add (selection, new NotifyDelegate (Notified));

			if (w is Stetic.IObjectWrapper)
				AddObjectWrapperProperties (w);
			else
				AddGObjectProperties (w);

			if (w is Stetic.IPropertySensitizer) {
				IPropertySensitizer sens = w as IPropertySensitizer;

				foreach (string prop in sens.InsensitiveProperties ()) {
					w = editors[prop] as Widget;
					if (w != null)
						w.Sensitive = false;
				}
				sens.SensitivityChanged += SensitivityChanged;
			}
		}

		public virtual ParamSpec LookupParamSpec (object obj, PropertyInfo info)
		{
			foreach (object attr in info.GetCustomAttributes (typeof (GLib.PropertyAttribute), false)) {
				PropertyAttribute pattr = (PropertyAttribute)attr;
				return ParamSpec.LookupObjectProperty (obj.GetType(), pattr.Name);
			}
			return null;
		}

		public void AddGObjectProperties (Widget w)
		{
			VBox group = AddGroup ("Properties");

			foreach (PropertyInfo info in w.GetType().GetProperties (BindingFlags.Instance | BindingFlags.Public)) {
				ParamSpec pspec = LookupParamSpec (w, info);
				if (pspec != null)
					AddToGroup (group, new PropertyDescriptor (w.GetType(), info.Name), w);
			}
		}

		public void AddObjectWrapperProperties (object obj)
		{
			foreach (PropertyGroup pgroup in ((IObjectWrapper)obj).PropertyGroups) {
				VBox group = AddGroup (pgroup.Name);
				foreach (PropertyDescriptor prop in pgroup.Properties)
					AddToGroup (group, prop, obj);
			}
		}
	}

	public class ChildPropertyGrid : PropertyGrid {

		public new void Select (WidgetBox wbox)
		{
			Clear ();

			Widget w = wbox.Child;
			if (w == null)
				return;

			Container parent = wbox.Parent as Container;
			if (parent == null)
				return;

			ContainerChild cc = parent[wbox];

			VBox group = AddGroup ("Properties");

			foreach (PropertyInfo info in cc.GetType().GetProperties (BindingFlags.Instance | BindingFlags.Public)) {
				foreach (object attr in info.GetCustomAttributes (false)) {
					ChildPropertyAttribute pattr = attr as ChildPropertyAttribute;
					if (pattr == null)
						continue;

					ParamSpec pspec = ParamSpec.LookupChildProperty (parent.GetType(), pattr.Name);
					if (pspec != null)
						AddToGroup (group, new PropertyDescriptor (cc.GetType (), info.Name), cc);
				}
			}
		}
	}
}
