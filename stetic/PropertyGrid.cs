using Gtk;
using Gdk;
using GLib;
using System;
using System.Reflection;

namespace Stetic {

	public class PropertyGrid : Gtk.VBox {

		SizeGroup sgroup;

		public PropertyGrid () : base (false, 6)
		{
			BorderWidth = 2;

			sgroup = new SizeGroup (SizeGroupMode.Horizontal);
			NoSelection ();
		}

		protected void Clear ()
		{
			foreach (Widget w in Children)
				Remove (w);
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

		protected void AddToGroup (VBox group, PropertyDescriptor prop, ParamSpec pspec, object obj)
		{
			HBox box = new HBox (false, 6);

			Label label;

			label = new Label ("    ");
			box.PackStart (label, false, false, 0);

			label = new Label (pspec != null ? pspec.Nick : prop.Name);
			label.UseMarkup = true;
			label.Justify = Justification.Left;
			label.Xalign = 0;
			box.PackStart (label, true, true, 0);

			Widget rep = PropertyEditors.MakeEditor (prop, pspec, obj);
			if (rep != null) {
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

		public void Select (WidgetBox wbox)
		{
			Clear ();

			Widget w = wbox.Child;
			if (w == null)
				return;

			if (w is Stetic.IObjectWrapper)
				AddObjectWrapperProperties (w);
			else
				AddGObjectProperties (w);
		}

		public virtual ParamSpec LookupParamSpec (object obj, PropertyInfo info)
		{
			foreach (object attr in info.GetCustomAttributes (typeof (GLib.PropertyAttribute), false)) {
				PropertyAttribute pattr = (PropertyAttribute)attr;
				return ParamSpec.LookupObjectProperty ((GLib.Object)obj, pattr.Name);
			}
			return null;
		}

		public void AddGObjectProperties (Widget w)
		{
			VBox group = AddGroup ("Properties");

			foreach (PropertyInfo info in w.GetType().GetProperties (BindingFlags.Instance | BindingFlags.Public)) {
				ParamSpec pspec = LookupParamSpec (w, info);
				if (pspec != null)
					AddToGroup (group, new PropertyDescriptor (w.GetType(), info.Name), pspec, w);
			}
		}

		public void AddObjectWrapperProperties (object obj)
		{
			foreach (PropertyGroup pgroup in ((IObjectWrapper)obj).PropertyGroups) {
				VBox group = AddGroup (pgroup.Name);
				foreach (PropertyDescriptor prop in pgroup.Properties) {
					ParamSpec pspec = LookupParamSpec (prop.PropertyObject (obj), prop.Info);
					AddToGroup (group, prop, pspec, obj);
				}
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

					ParamSpec pspec = ParamSpec.LookupChildProperty (parent, pattr.Name);
					if (pspec != null)
						AddToGroup (group, new PropertyDescriptor (cc.GetType (), info.Name), pspec, cc);
				}
			}
		}
	}
}
