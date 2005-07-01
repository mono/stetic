using Gtk;
using Gdk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class Palette : Gtk.VBox, IComparer {

		Hashtable groups;

		class Group : Gtk.Expander {
		
			private Gtk.Alignment align;
			private Gtk.VBox vbox;
			
			public Group (string name) : base ("<b>" + name + "</b>")
			{
				vbox = new VBox (false, 5);
				
				align = new Gtk.Alignment (0, 0, 0, 0);
				align.SetPadding (0, 0, 20, 0);
				align.Child = vbox;

				UseMarkup = true;
				Expanded = true;
				Child = align;
			}
			
			public void Append (Widget w)
			{
				vbox.PackStart (w, false, false, 0);
			} 
		}

		public Palette (Project project) : base (false, 2)
		{
			groups = new Hashtable ();

			AddOrGetGroup ("widget", "Widgets");
			AddOrGetGroup ("container", "Containers");
			AddOrGetGroup ("toolbaritem", "Toolbar Items");
			AddOrGetGroup ("window", "Windows");

			ArrayList classes = new ArrayList ();
			foreach (ClassDescriptor klass in Registry.AllClasses)
				classes.Add (klass);
			classes.Sort (this);

			foreach (ClassDescriptor klass in classes) {
				if (klass.Deprecated || klass.Category == "internal" || klass.Icon == null)
					continue;

				WidgetFactory factory;
				if (klass.Category == "window")
					factory = new WindowFactory (project, klass);
				else
					factory = new WidgetFactory (project, klass);

				AddOrGetGroup(klass.Category).Append (factory);
			}
		}

		public int Compare (object x, object y)
		{
			return string.Compare (((ClassDescriptor)x).Label,
					       ((ClassDescriptor)y).Label);
		}

		private Group AddOrGetGroup (string id, string name)
		{
			Group group = (Group) groups[id];

			if (group == null) {
				group = new Group (name);
				PackStart (group, false, false, 0);
				groups.Add (id, group);
			}

			return group;
		}

		private Group AddOrGetGroup (string name)
		{
			return AddOrGetGroup (name, name);
		}
	}
}
