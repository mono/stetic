
using System;

namespace Stetic
{
	public class ComponentType
	{
		Application app;
		string name;
		string description;
		string className;
		Gdk.Pixbuf icon;
		static ComponentType unknown;
		
		internal ComponentType (Application app, string name, string desc, string className, Gdk.Pixbuf icon)
		{
			this.app = app;
			this.name = name;
			this.description = desc;
			this.icon = icon;
			this.className= className;
		}
		
		public string Name {
			get { return name; }
		}
		
		public string ClassName {
			get { return className; }
		}
		
		public string Description {
			get { return description; }
		}
		
		public Gdk.Pixbuf Icon {
			get { return icon; }
		}
		
		internal static ComponentType Unknown {
			get {
				if (unknown == null) {
					Gtk.IconSet iset = Gtk.IconFactory.LookupDefault (Gtk.Stock.MissingImage);
					Gdk.Pixbuf px = iset.RenderIcon (new Gtk.Style (), Gtk.TextDirection.Ltr, Gtk.StateType.Normal, Gtk.IconSize.Menu, null, "");
					unknown = new ComponentType (null, "Unknown", "Unknown", "", px);
				}
				return unknown;
			}
		}
		
		public object[] InitializationValues {
			get {
				if (app == null)
					return new object [0];
				return app.Backend.GetClassDescriptorInitializationValues (name);
			}
		}
	}
}
