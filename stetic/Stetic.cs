using Gtk;
using Gnome;
using System;
using System.Reflection;

namespace Stetic {

	public class SteticMain  {

		static Gnome.Program program;

		public static int Main (string[] args) {
			Gtk.Window win;
			Gtk.Box vbox, hbox;
			Gtk.Paned vpaned;
			Gtk.ScrolledWindow scwin;

			program = new Gnome.Program ("Stetic", "0.0", Modules.UI, args);

			Project = new Project ();

			win = new Gtk.Window ("Stetic");
			win.DeleteEvent += Window_Delete;

			vbox = new Gtk.VBox (false, 6);
			win.Add (vbox);

			Gtk.MenuBar menu_bar = new Gtk.MenuBar ();
			Gtk.MenuItem file_menu_item = new Gtk.MenuItem ("File");
			menu_bar.Append (file_menu_item);
			Gtk.Menu file_menu = new Gtk.Menu ();
			file_menu_item.Submenu = file_menu;
			Gtk.MenuItem load_glade_menu_item = new Gtk.MenuItem ("Load Glade...");
			load_glade_menu_item.Activated += new EventHandler (OnLoadActivated);
			file_menu.Append (load_glade_menu_item);
			vbox.PackStart (menu_bar, false, false, 0);

			hbox = new Gtk.HBox (false, 6);
			vbox.PackStart (hbox, true, true, 0);

			Stetic.Palette palette = new Stetic.Palette (Project);
			AssemblyName an = new AssemblyName ();
			an.Name = "libstetic";
			palette.AddWidgets (System.Reflection.Assembly.Load (an));
			hbox.PackStart (palette, false, true, 0);

			vpaned = new Gtk.VPaned ();
			hbox.PackStart (vpaned, true, true, 0);
			
			ProjectView = new ProjectView (Project);
			scwin = new Gtk.ScrolledWindow ();
			scwin.Add (ProjectView);
			vpaned.Pack1 (scwin, false, false);

			scwin = new Gtk.ScrolledWindow ();
			scwin.SetPolicy (Gtk.PolicyType.Never, Gtk.PolicyType.Automatic);
			Properties = new Stetic.PropertyGrid ();
			Properties.Show ();
			scwin.AddWithViewport (Properties);
			Gtk.Viewport vp = (Gtk.Viewport)scwin.Child;
			vp.ShadowType = Gtk.ShadowType.None;
			vpaned.Pack2 (scwin, true, false);

			win.DefaultHeight = 480;
			win.DefaultWidth = 640;
			win.ShowAll ();

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

		public static void NoSelection ()
		{
			Properties.NoSelection ();
		}

		public static void Select (IWidgetSite site)
		{
			Properties.Select (site);
		}

	}
}
