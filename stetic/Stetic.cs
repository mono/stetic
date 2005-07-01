using Gtk;
using Gnome;
using System;
using System.Reflection;

namespace Stetic {

	public class SteticMain  {

		static Gnome.Program program;

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

			if (ProjectView.Parent is Gtk.Viewport &&
			    ProjectView.Parent.Parent is Gtk.ScrolledWindow) {
				Gtk.Viewport viewport = (Gtk.Viewport)ProjectView.Parent;
				Gtk.ScrolledWindow scrolled = (Gtk.ScrolledWindow)viewport.Parent;
				viewport.Remove (ProjectView);
				scrolled.Remove (viewport);
				scrolled.Add (ProjectView);
			}

			foreach (Gtk.Widget w in glade.GetWidgetPrefix ("")) {
				if (w is Gtk.Window)
					w.ShowAll ();
			}

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

		internal static void ImportGlade (object obj, EventArgs e)
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

		internal static void ExportGlade (object obj, EventArgs e)
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

		internal static void Cut (object obj, EventArgs e)
		{
			Console.WriteLine ("Cut");
		}

		internal static void Copy (object obj, EventArgs e)
		{
			Console.WriteLine ("Copy");
		}

		internal static void Paste (object obj, EventArgs e)
		{
			Console.WriteLine ("Paste");
		}

		internal static void Delete (object obj, EventArgs e)
		{
			Console.WriteLine ("Delete");
		}

		internal static void About (object obj, EventArgs e)
		{
			Gtk.AboutDialog.SetUrlHook (ActivateUrl);
			Gtk.AboutDialog about = new Gtk.AboutDialog ();
			about.Name = "Stetic";
			about.Version = "0.0.0";
			about.Comments = "A GNOME and Gtk GUI designer";
			about.Authors = new string[] { "Dan Winship" };
			about.Copyright = "Copyright 2004, 2005 Novell, Inc.";
			about.Website = "http://mono-project.com/Stetic";
			about.Run ();
		}

		static void ActivateUrl (Gtk.AboutDialog about, string url)
		{
			Gnome.Url.Show (url);
		}

		internal static void Quit (object obj, EventArgs e)
		{
			program.Quit ();
		}

		internal static void Window_Delete (object obj, DeleteEventArgs args) {
			program.Quit ();
			args.RetVal = true;
		}
	}
}
