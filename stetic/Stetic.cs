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

			Gtk.VBox vbox = new Gtk.VBox (false, 0);
			Gtk.MenuBar menu_bar = new Gtk.MenuBar ();
			Gtk.MenuItem file_menu_item = new Gtk.MenuItem ("File");
			menu_bar.Append (file_menu_item);
			Gtk.Menu file_menu = new Gtk.Menu ();
			file_menu_item.Submenu = file_menu;
			Gtk.MenuItem load_glade_menu_item = new Gtk.MenuItem ("Load Glade...");
			load_glade_menu_item.Activated += new EventHandler (OnLoadActivated);
			file_menu.Append (load_glade_menu_item);
			vbox.PackStart (menu_bar, false, false, 0);

			Gtk.VPaned vpaned = new Gtk.VPaned ();
			Gtk.HBox hbox = new Gtk.HBox (false, 2);
			
			vbox.PackStart (vpaned, true, true, 0);
			
			notebook = new Gtk.Notebook ();
			win.Add (vbox);

			palette = new Stetic.Palette ();
			AssemblyName an = new AssemblyName ();
			an.Name = "libstetic";
			palette.AddWidgets (System.Reflection.Assembly.Load (an));
			hbox.PackStart (palette, false, true, 2);

			Project = new Project ();
			ProjectView = new ProjectView (Project);
			Gtk.ScrolledWindow project_scroller = new Gtk.ScrolledWindow ();
			project_scroller.Add (ProjectView);
			vpaned.Pack1 (project_scroller, true, true);

			Gtk.ScrolledWindow property_scroller = new Gtk.ScrolledWindow ();
			Properties = new Stetic.PropertyGrid ();
			Properties.Show ();
			property_scroller.AddWithViewport (Properties);
			notebook.AppendPage (property_scroller, new Label ("Properties"));

			ChildProperties = new Stetic.ChildPropertyGrid ();
			ChildProperties.Show ();
			notebook.AppendPage (ChildProperties, new Label ("Packing"));

			hbox.PackStart (notebook, true, true, 2);
			vpaned.Pack2 (hbox, true, true);

			Stetic.Grid.Connect (Properties, ChildProperties);
			win.DefaultHeight = 480;
			win.DefaultWidth = 640;
			win.ShowAll ();

			notebook.Page = 0;

			program.Run ();
			return 0;
		}

		static void OnLoadActivated (object obj, EventArgs e)
		{
			FileChooserDialog dialog = new FileChooserDialog ("Load glade file", null, FileChooserAction.Open, Gtk.Stock.Cancel, Gtk.ResponseType.Cancel, Gtk.Stock.Open, Gtk.ResponseType.Ok);
			int response = dialog.Run ();
			if (response == (int)Gtk.ResponseType.Ok) {
				GladeImport.Load (dialog.Filename, Project);
			}
			dialog.Hide ();
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
