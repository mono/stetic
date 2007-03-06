
using System;

namespace Stetic
{
	public class ComponentType
	{
		Application app;
		string name;
		string description;
		string className;
		string category;
		Gdk.Pixbuf icon;
		ActionComponent action;
		static ComponentType unknown;
		
		internal ComponentType (Application app, string name, string desc, string className, string category, Gdk.Pixbuf icon)
		{
			this.app = app;
			this.name = name;
			this.description = desc;
			this.icon = icon;
			this.className = className;
			this.category = category;
		}
		
		internal ComponentType (Application app, ActionComponent action)
		{
			this.action = action;
			this.app = app;
			this.name = action.Name;
			this.description = name;
			this.icon = action.Icon;
			this.className = "Gtk.Action";
			this.category = "Actions / " + action.ActionGroup.Name;
		}
		
		public string Name {
			get { return name; }
		}
		
		public string ClassName {
			get { return className; }
		}
		
		public string Category {
			get { return category; }
		}
		
		public string Description {
			get { return description; }
		}
		
		public Gdk.Pixbuf Icon {
			get { return icon; }
		}
		
		internal ActionComponent Action {
			get { return action; }
		}
		
		internal static ComponentType Unknown {
			get {
				if (unknown == null) {
					Gtk.IconSet iset = Gtk.IconFactory.LookupDefault (Gtk.Stock.MissingImage);
					Gdk.Pixbuf px = iset.RenderIcon (new Gtk.Style (), Gtk.TextDirection.Ltr, Gtk.StateType.Normal, Gtk.IconSize.Menu, null, "");
					unknown = new ComponentType (null, "Unknown", "Unknown", "", "", px);
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
