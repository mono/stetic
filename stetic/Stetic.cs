using Gtk;
using System;
using System.Reflection;

namespace Stetic {

	public class SteticMain  {

		public static Gnome.Program Program;

		static Stetic.Palette Palette;
		static Stetic.Project Project;
		static Stetic.ProjectView ProjectView;
		static Stetic.PropertyGrid Properties;

		public static Stetic.UIManager UIManager;
		public static Gtk.Window MainWindow;

		public static int Main (string[] args) {
			Program = new Gnome.Program ("Stetic", "0.0", Gnome.Modules.UI, args);

			Project = new Project ();

			Palette = new Stetic.Palette (Project);
			ProjectView = new Stetic.ProjectView (Project);
			Properties = new Stetic.PropertyGrid (Project);
			UIManager = new Stetic.UIManager (Project);

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
				Gtk.Window win = w as Gtk.Window;
				if (win != null) {
					win.AddAccelGroup (UIManager.AccelGroup);
					win.ShowAll ();
				}
			}
			MainWindow = (Gtk.Window)Properties.Toplevel;

#if GTK_SHARP_2_6
			// This is needed for both our own About dialog and for ones
			// the user constructs
			Gtk.AboutDialog.SetUrlHook (ActivateUrl);
#endif

			Program.Run ();
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
			else if (name == "MenuBar")
				return UIManager.MenuBar;
			else
				return null;
		}

#if GTK_SHARP_2_6
		static void ActivateUrl (Gtk.AboutDialog about, string url)
		{
			Gnome.Url.Show (url);
		}
#endif

		internal static void Window_Delete (object obj, DeleteEventArgs args) {
			Program.Quit ();
			args.RetVal = true;
		}
	}
}
