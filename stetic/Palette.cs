using Gtk;
using Gdk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class Palette : Gtk.ScrolledWindow {

		Gtk.VBox main;
		Hashtable groups;
		Project project;

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
		
		public Palette (Project project)
		{
			this.project = project;

			HscrollbarPolicy = Gtk.PolicyType.Never;
			VscrollbarPolicy = Gtk.PolicyType.Automatic;
			ShadowType = ShadowType.None;
			
			groups = new Hashtable ();
			
			main = new VBox (false, 2);
			AddWithViewport (main);

			AddOrGetGroup ("Widgets");
			AddOrGetGroup ("Containers");
			AddOrGetGroup ("Windows");
		}
		
		private Group AddOrGetGroup(string name)
		{
			Group group = (Group) groups[name];
			
			if (group == null) {
				group = new Group (name); 
				main.PackStart (group, false, false, 0);
				groups.Add (name, group);
			}
			
			return group; 
		}

		public static Pixbuf IconForType (Type type)
		{
			foreach (object attr in type.GetCustomAttributes (typeof (ObjectWrapperAttribute), false)) {
				ObjectWrapperAttribute owattr = attr as ObjectWrapperAttribute;

				try {
					return new Gdk.Pixbuf (type.Assembly, owattr.IconName);
				} catch {
					;
				}
			}
			return Gdk.Pixbuf.LoadFromResource ("missing.png");
		}

		public void AddWidget (Assembly assem, Type type)
		{
			foreach (object attr in type.GetCustomAttributes (typeof (ObjectWrapperAttribute), false)) {
				Stetic.ObjectWrapper.Register (type);

				ObjectWrapperAttribute owattr = attr as ObjectWrapperAttribute;
				if (owattr.Deprecated)
					return;

				Pixbuf icon = Palette.IconForType (type);

				switch (owattr.Type) {
				case ObjectWrapperType.Container:
					AddOrGetGroup("Containers").Append (new WidgetFactory (project, owattr.Name, icon, type));
					break;

				case ObjectWrapperType.Window:
					AddOrGetGroup("Windows").Append (new WindowFactory (project, owattr.Name, icon, type));
					break;

				default:
					AddOrGetGroup("Widgets").Append (new WidgetFactory (project, owattr.Name, icon, type));
					break;
				}
			}
		}

		public void AddWidgets (Assembly assem)
		{
			foreach (Type type in assem.GetExportedTypes ())
				AddWidget (assem, type);

			ShowAll ();
		}			
	}
}
