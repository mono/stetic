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

			AddOrGetGroup (Stetic.ObjectWrapperType.Widget);
			AddOrGetGroup (Stetic.ObjectWrapperType.Container);
			AddOrGetGroup (Stetic.ObjectWrapperType.ToolbarItem);
			AddOrGetGroup (Stetic.ObjectWrapperType.Window);
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
				if (owattr.Deprecated || owattr.Type == ObjectWrapperType.Internal)
					return;

				Pixbuf icon = Palette.IconForType (type);
				WidgetFactory factory;

				if (owattr.Type == ObjectWrapperType.Window)
					factory = new WindowFactory (project, owattr.Name, icon, type);
				else
					factory = new WidgetFactory (project, owattr.Name, icon, type);

				AddOrGetGroup(owattr.Type).Append (factory);
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
