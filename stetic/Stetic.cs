using Gtk;
using Gnome;
using System;
using System.Reflection;

namespace Stetic {

	public class SteticMain  {

		static Gnome.Program program;

		public static int Main (string[] args) {
			Gtk.Window win;
			Gtk.Notebook notebook;
			Stetic.Palette palette;

			program = new Gnome.Program ("Stetic", "0.0", Modules.UI, args);

			win = new Gtk.Window ("Stetic");
			win.DeleteEvent += Window_Delete;
			notebook = new Gtk.Notebook ();
			win.Add (notebook);

			palette = new Stetic.Palette ();
			AssemblyName an = new AssemblyName ();
			an.Name = "libstetic";
			palette.AddWidgets (System.Reflection.Assembly.Load (an));
			notebook.AppendPage (palette, new Label ("Palette"));

			Project = new Project ();
			ProjectView = new ProjectView (Project);
			notebook.AppendPage (ProjectView, new Label ("Project"));

			Properties = new Stetic.PropertyGrid ();
			Properties.Show ();
			notebook.AppendPage (Properties, new Label ("Properties"));

			ChildProperties = new Stetic.ChildPropertyGrid ();
			ChildProperties.Show ();
			notebook.AppendPage (ChildProperties, new Label ("Packing"));

			Stetic.Grid.Connect (Properties, ChildProperties);
			win.ShowAll ();

			program.Run ();
			return 0;
		}

		static void Window_Delete (object obj, DeleteEventArgs args) {
			program.Quit ();
			args.RetVal = true;
		}

		public static Stetic.Project Project;
		static Stetic.ProjectView ProjectView;

		static Stetic.PropertyGrid Properties;
		static Stetic.ChildPropertyGrid ChildProperties;

		public static void NoSelection ()
		{
			Properties.NoSelection ();
			ChildProperties.NoSelection ();
		}

		public static void Select (IWidgetSite site)
		{
			Properties.Select (site);
			ChildProperties.Select (site);
		}

	}
}
