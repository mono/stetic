using Gtk;
using System;
using System.Collections;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;

namespace Stetic {

	public class SteticMain  {

		public static Gnome.Program Program;

		static Stetic.Palette Palette;
		static Stetic.Project Project;
		static Stetic.ProjectView ProjectView;
		static Stetic.PropertyGrid Properties;
		static Stetic.SignalsEditor Signals;

		public static Stetic.UIManager UIManager;
		public static Gtk.Window MainWindow;
		
		static string language = "C#";
		static ArrayList libraries = new ArrayList ();
		

		public static int Main (string[] args)
		{
			Program = new Gnome.Program ("Stetic", "0.0", Gnome.Modules.UI, args);
			
			int n = 0;
			
			while (n < args.Length) {
				string arg = args[n];
				if (arg.StartsWith ("--language:"))
					language = arg.Substring (11);
				else if (arg.StartsWith ("-l:"))
					language = arg.Substring (3);
				else if (arg.StartsWith ("-lib:"))
					libraries.Add (arg.Substring (5));
				else if (arg.StartsWith ("--library:"))
					libraries.Add (arg.Substring (10));
				else if (arg == "--generate" || arg == "-g")
					break;
				n++;
			}
			
			if (args.Length - n > 2) {
				if (args [n] == "--generate" || args [n] == "-g")
					return GenerateCode (args [n+1], args, n+2);
			}
			
			if (args.Length == 1 && args [0] == "--help") {
				Console.WriteLine ("Stetic - A GTK User Interface Builder"); 
				Console.WriteLine ("Usage:");
				Console.WriteLine ("\tstetic [<file>]");
				Console.WriteLine ("\tstetic [--language:<language>] [-lib:<library>...] --generate <sourceFile> <projectFile> ...");
				return 0;
			}
			
			return RunApp (args);
		}
		
		static int GenerateCode (string file, string[] args, int n)
		{
			foreach (string lib in libraries)
				Registry.RegisterWidgetLibrary (new AssemblyWidgetLibrary (lib));
	
			Project[] projects = new Project [args.Length - n];
			for (int i=n; i<args.Length; i++) {
				projects [i - n] = new Project ();
				if (System.IO.Path.GetExtension (args [i]) == ".glade")
					GladeFiles.Import (projects [i - n], args [i]);
				else
					projects [i - n].Load (args [i]);
			}
			CodeDomProvider provider = GetProvider (language);
			CodeGenerator.GenerateProjectCode (file, "Stetic", provider, projects);
			return 0;
		}
		
		static CodeDomProvider GetProvider (string language)
		{
			return new Microsoft.CSharp.CSharpCodeProvider ();
		}
		
		static int RunApp (string[] args)
		{
			Project = new Project ();
			Project.WidgetAdded += OnWidgetAdded;
			Project.Selected += OnSelectionChanged;

			Palette = new Stetic.Palette (Project);
			ProjectView = new Stetic.ProjectView (Project);
			Properties = new Stetic.PropertyGrid (Project);
			Signals = new Stetic.SignalsEditor (Project);
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
			else if (name == "SignalsEditor")
				return Signals;
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
		
		static void OnWidgetAdded (object s, Wrapper.WidgetEventArgs args)
		{
			if (args.Widget is Wrapper.Window) {
				((Gtk.Window)args.Widget.Wrapped).Present ();
			}
		}
		
		static void OnSelectionChanged (Gtk.Widget selection, ProjectNode node)
		{
			if (selection is Gtk.Window)
				((Gtk.Window)selection).Present ();
		}
	}
}
