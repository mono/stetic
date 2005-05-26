using Gtk;
using Gnome;
using System;
using System.Reflection;

namespace Stetic {

	public class SteticMain  {

		static Gnome.Program program;

		[Glade.Widget] static Gtk.Window MainWindow;
		static Stetic.Palette Palette;
		static Stetic.Project Project;
		static Stetic.ProjectView ProjectView;
		static Stetic.PropertyGrid Properties;

		public static int Main (string[] args) {
			program = new Gnome.Program ("Stetic", "0.0", Modules.UI, args);

			Project = new Project ();

			Palette = new Stetic.Palette (Project);
			ProjectView = new Stetic.ProjectView (Project);
			Properties = new Stetic.PropertyGrid (Project);

			Glade.XML.CustomHandler = CustomWidgetHandler;
			Glade.XML glade = new Glade.XML ("stetic.glade", null);
			glade.Autoconnect (typeof (SteticMain));

			MainWindow.DeleteEvent += Window_Delete;

			AssemblyName an = new AssemblyName ();
			an.Name = "libstetic";
			Palette.AddWidgets (System.Reflection.Assembly.Load (an));

			Gtk.MenuBar menubar = (Gtk.MenuBar)glade.GetWidget ("Menubar");
			Gtk.MenuItem item = new Gtk.MenuItem ("File");
			menubar.Append (item);
			Gtk.Menu fileMenu = new Gtk.Menu ();
			item.Submenu = fileMenu;
			item = new Gtk.MenuItem ("Import from Glade File...");
			item.Activated += ImportGlade;
			fileMenu.Append (item);
			item = new Gtk.MenuItem ("Export to Glade File...");
			item.Activated += ExportGlade;
			fileMenu.Append (item);

			MainWindow.ShowAll ();

			program.Run ();
			return 0;
		}

		static Gtk.Widget CustomWidgetHandler (Glade.XML xml, string func_name,
						       string name, string string1, string string2,
						       int int1, int int2)
		{
			if (name == "Palette")
				return Palette;
			else if (name == "ProjectView")
				return ProjectView;
			else if (name == "PropertyGrid")
				return Properties;
			else
				return null;
		}

		static void ImportGlade (object obj, EventArgs e)
		{
			FileChooserDialog dialog =
				new FileChooserDialog ("Import from Glade File", null, FileChooserAction.Open,
						       Gtk.Stock.Cancel, Gtk.ResponseType.Cancel,
						       Gtk.Stock.Open, Gtk.ResponseType.Ok);
			int response = dialog.Run ();
			if (response == (int)Gtk.ResponseType.Ok)
				GladeFiles.Import (Project, dialog.Filename);
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
				GladeFiles.Export (Project, dialog.Filename);
			dialog.Hide ();
		}

		static void Window_Delete (object obj, DeleteEventArgs args) {
			program.Quit ();
			args.RetVal = true;
		}
	}
}
