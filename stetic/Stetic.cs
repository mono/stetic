using Gtk;
using Gnome;
using System;
using System.Reflection;

namespace Stetic {

	public class SteticMain  {

		static Gnome.Program program;

		static Stetic.Project Project;
		static Stetic.ProjectView ProjectView;
		static Stetic.PropertyGrid Properties;

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
			Gtk.MenuItem item = new Gtk.MenuItem ("File");
			menu_bar.Append (item);
			Gtk.Menu file_menu = new Gtk.Menu ();
			item.Submenu = file_menu;
			item = new Gtk.MenuItem ("Import from Glade File...");
			item.Activated += ImportGlade;
			file_menu.Append (item);
			item = new Gtk.MenuItem ("Export to Glade File...");
			item.Activated += ExportGlade;
			file_menu.Append (item);
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
			scwin.ShadowType = ShadowType.In;
			scwin.Add (ProjectView);
			vpaned.Pack1 (scwin, false, false);

			scwin = new Gtk.ScrolledWindow ();
			scwin.SetPolicy (Gtk.PolicyType.Never, Gtk.PolicyType.Automatic);
			Properties = new Stetic.PropertyGrid (Project);
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

		static void ImportGlade (object obj, EventArgs e)
		{
			FileChooserDialog dialog =
				new FileChooserDialog ("Import from Glade File", null, FileChooserAction.Open,
						       Gtk.Stock.Cancel, Gtk.ResponseType.Cancel,
						       Gtk.Stock.Open, Gtk.ResponseType.Ok);
			int response = dialog.Run ();
			if (response == (int)Gtk.ResponseType.Ok)
				Glade.Import (Project, dialog.Filename);
			dialog.Hide ();
		}

		static void ExportGlade (object obj, EventArgs e)
		{
			FileChooserDialog dialog =
				new FileChooserDialog ("Export to Glade File", null, FileChooserAction.Save,
						       Gtk.Stock.Cancel, Gtk.ResponseType.Cancel,
						       Gtk.Stock.Save, Gtk.ResponseType.Ok);
			int response = dialog.Run ();
			if (response == (int)Gtk.ResponseType.Ok)
				Glade.Export (Project, dialog.Filename);
			dialog.Hide ();
		}

		static void Window_Delete (object obj, DeleteEventArgs args) {
			program.Quit ();
			args.RetVal = true;
		}
	}
}
