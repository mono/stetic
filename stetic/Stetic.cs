using Gtk;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Xml.Serialization;
using Mono.Unix;

namespace Stetic {

	public class SteticMain  {

		static Gnome.Program Program;

		static Stetic.Palette Palette;
		static Stetic.Project Project;
		static Stetic.ProjectView ProjectView;
		static Stetic.SignalsEditor Signals;
		static Gtk.Notebook WidgetNotebook; 
		static Stetic.WidgetPropertyTree propertyTree;

		public static Stetic.UIManager UIManager;
		public static Gtk.Window MainWindow;
		
		static string language = "C#";
		static ArrayList libraries = new ArrayList ();
		
		static Hashtable openWindows = new Hashtable ();
		static Configuration configuration;
		

		public static int Main (string[] args)
		{
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
			
			if (args.Length == 1 && args [0] == "--help") {
				Console.WriteLine (Catalog.GetString ("Stetic - A GTK User Interface Builder")); 
				Console.WriteLine (Catalog.GetString ("Usage:"));
				Console.WriteLine ("\tstetic [<file>]");
				Console.WriteLine ("\tstetic [--language:<language>] [-lib:<library>...] --generate <sourceFile> <projectFile> ...");
				return 0;
			}
			
			Program = new Gnome.Program ("Stetic", "0.0", Gnome.Modules.UI, args);
			
			if (args.Length - n > 2) {
				if (args [n] == "--generate" || args [n] == "-g")
					return GenerateCode (args [n+1], args, n+2);
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
			Project.ActionGroups.Add (new Stetic.Wrapper.ActionGroup ("Global"));
			Project.WidgetAdded += OnWidgetAdded;
			Project.WidgetRemoved += OnWidgetRemoved;
			Project.SelectionChanged += OnSelectionChanged;
			Project.ModifiedChanged += OnProjectModified;

			Palette = new Stetic.Palette (Project);
			ProjectView = new Stetic.ProjectView (Project);
			Signals = new Stetic.SignalsEditor (Project);
			UIManager = new Stetic.UIManager (Project);
			propertyTree = new Stetic.WidgetPropertyTree (Project);

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
			MainWindow = (Gtk.Window)Palette.Toplevel;
			WidgetNotebook = (Gtk.Notebook) glade ["notebook"];
			ProjectView.WidgetActivated += OnWidgetActivated;

#if GTK_SHARP_2_6
			// This is needed for both our own About dialog and for ones
			// the user constructs
			Gtk.AboutDialog.SetUrlHook (ActivateUrl);
#endif

			if (args.Length > 0) {
				LoadProject (args [0]);
			}

			ReadConfiguration ();
			
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
				return propertyTree;
			else if (name == "SignalsEditor")
				return Signals;
			else if (name == "MenuBar")
				return UIManager.MenuBar;
			else if (name == "Toolbar")
				return UIManager.Toolbar;
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
			args.RetVal = true;
			Quit ();
		}
		
		static void OnWidgetAdded (object s, Wrapper.WidgetEventArgs args)
		{
			if (args.Widget.IsTopLevel) {
				OpenWindow (args.Widget as Gtk.Container);
			}
		}
		
		static void OnWidgetRemoved (object s, Wrapper.WidgetEventArgs args)
		{
			CloseWindow (args.Widget as Gtk.Container);
		}
		
		static void OnSelectionChanged (object s, Wrapper.WidgetEventArgs args)
		{
			Stetic.Wrapper.Container wc = args.WidgetWrapper as Stetic.Wrapper.Container;
			if (wc != null && wc.IsTopLevel && IsWindowOpen ((Gtk.Container) args.Widget))
				OpenWindow ((Gtk.Container) args.Widget);
		}
		
		static void OnWidgetActivated (object s, Wrapper.WidgetEventArgs args)
		{
			Stetic.Wrapper.Widget w = args.WidgetWrapper;
			while (!w.IsTopLevel)
				w = Stetic.Wrapper.Container.LookupParent (w.Wrapped);
			OpenWindow ((Gtk.Container) w.Wrapped);
		}
		
		static void OnProjectModified (object s, EventArgs a)
		{
			string title = "Stetic - " + Path.GetFileName (Project.FileName);
			if (Project.Modified)
				title += "*";
			MainWindow.Title = title;
		}
		
		static bool IsWindowOpen (Gtk.Container widget)
		{
			Gtk.Widget w = openWindows [widget] as Gtk.Widget;
			return w != null && w.Visible;
		}
		
		static void OpenWindow (Gtk.Container widget)
		{
			Gtk.Widget page = (Gtk.Widget) openWindows [widget];
			if (page != null) {
				page.Show ();
				WidgetNotebook.Page = WidgetNotebook.PageNum (page);
			}
			else {
				Stetic.Wrapper.Container wc = Stetic.Wrapper.Container.Lookup (widget);
				DesignerView view = new DesignerView (Project, widget);
				
				// Tab label
				
				HBox tabLabel = new HBox ();
				tabLabel.PackStart (new Gtk.Image (wc.ClassDescriptor.Icon), true, true, 0);
				tabLabel.PackStart (new Label (widget.Name), true, true, 3);
				Button b = new Button (new Gtk.Image ("gtk-close", IconSize.Menu));
				b.Relief = Gtk.ReliefStyle.None;
				b.WidthRequest = b.HeightRequest = 24;
				
				b.Clicked += delegate (object s, EventArgs a) {
					view.Hide ();
					WidgetNotebook.QueueResize ();
				};
				
				tabLabel.PackStart (b, false, false, 0);
				tabLabel.ShowAll ();
				
				// Notebook page
				
				int p = WidgetNotebook.AppendPage (view, tabLabel);
				view.ShowAll ();
				openWindows [widget] = view;
				WidgetNotebook.Page = p;
			}
		}
		
		static void CloseWindow (Gtk.Container widget)
		{
			if (widget != null) {
				Gtk.Widget page = (Gtk.Widget) openWindows [widget];
				if (page != null) {
					WidgetNotebook.Remove (page);
					openWindows.Remove (widget);
					page.Dispose ();
				}
			}
		}
		
		public static void LoadProject (string file)
		{
			try {
				if (!CloseProject ())
					return;

				Project.Load (file);
				
				string title = "Stetic - " + Path.GetFileName (file);
				MainWindow.Title = title;
				
			} catch (Exception ex) {
				string msg = string.Format ("The file '{0}' could not be loaded.", file);
				msg += " " + ex.Message;
				Gtk.MessageDialog dlg = new Gtk.MessageDialog (null, Gtk.DialogFlags.Modal, Gtk.MessageType.Error, ButtonsType.Close, msg);
				dlg.Run ();
				dlg.Destroy ();
			}
		}
		
		public static bool SaveProject ()
		{
			if (Project.FileName == null)
				return SaveProjectAs ();
			else {
				try {
					Project.Save (Project.FileName);
					Project.Modified = false;
					return true;
				} catch (Exception ex) {
					ReportError (Catalog.GetString ("The project could not be saved."), ex);
					return false;
				}
			}
		}
		
		public static bool SaveProjectAs ()
		{
			FileChooserDialog dialog =
				new FileChooserDialog (Catalog.GetString ("Save Stetic File As"), null, FileChooserAction.Save,
						       Gtk.Stock.Cancel, Gtk.ResponseType.Cancel,
						       Gtk.Stock.Save, Gtk.ResponseType.Ok);

			if (Project.FileName != null)
				dialog.CurrentName = Project.FileName;

			int response = dialog.Run ();
			if (response == (int)Gtk.ResponseType.Ok) {
				string name = dialog.Filename;
				if (System.IO.Path.GetExtension (name) == "")
					name = name + ".stetic";
				try {
					Project.Save (name);
					Project.Modified = false;
					SteticMain.UIManager.AddRecentFile (name);
				} catch (Exception ex) {
					ReportError (Catalog.GetString ("The project could not be saved."), ex);
					return false;
				}
			}
			dialog.Hide ();
			return true;
		}
		
		
		public static bool CloseProject ()
		{
			if (Project.Modified) {
				string msg = Catalog.GetString ("Do you want to save the project before closing?");
				Gtk.MessageDialog dlg = new Gtk.MessageDialog (null, Gtk.DialogFlags.Modal, Gtk.MessageType.Error, ButtonsType.None, msg);
				dlg.AddButton (Catalog.GetString ("Close without saving"), Gtk.ResponseType.No);
				dlg.AddButton (Gtk.Stock.Cancel, Gtk.ResponseType.Cancel);
				dlg.AddButton (Gtk.Stock.Save, Gtk.ResponseType.Yes);
				Gtk.ResponseType res = (Gtk.ResponseType) dlg.Run ();
				dlg.Destroy ();
				
				if (res == Gtk.ResponseType.Cancel)
					return false;
					
				if (res == Gtk.ResponseType.Yes) {
					if (!SaveProject ())
						return false;
				}
			}
			
			object[] obs = new object [openWindows.Count];
			openWindows.Values.CopyTo (obs, 0);
			foreach (Gtk.Widget page in obs) {
				WidgetNotebook.Remove (page);
				page.Destroy ();
			}
				
			openWindows.Clear ();

			Project.Close ();
			MainWindow.Title = "Stetic";
			return true;
		}
		
		public static void Quit ()
		{
			SaveConfiguration ();
			Palette.Destroy ();
			Program.Quit ();
		}
		
		static void ReportError (string message, Exception ex)
		{
			string msg = message + " " + ex.Message;
			Gtk.MessageDialog dlg = new Gtk.MessageDialog (MainWindow, Gtk.DialogFlags.Modal, Gtk.MessageType.Error, ButtonsType.Close, msg);
			dlg.Run ();
			dlg.Destroy ();
		}
		
		static void ReadConfiguration ()
		{
			string file = Path.Combine (SteticMain.ConfigDir, "configuration.xml");
			configuration = null;
			
			if (File.Exists (file)) {
				try {
					using (StreamReader sr = new StreamReader (file)) {
						XmlSerializer ser = new XmlSerializer (typeof (Configuration));
						configuration = (Configuration) ser.Deserialize (sr);
					}
				} catch {
					// Ignore exceptions while reading the recents file
				}
			}
			
			if (configuration != null) {
				MainWindow.Move (configuration.WindowX, configuration.WindowY);
				MainWindow.Resize (configuration.WindowWidth, configuration.WindowHeight);
				if (configuration.WindowState == Gdk.WindowState.Maximized)
					MainWindow.Maximize ();
				else if (configuration.WindowState == Gdk.WindowState.Iconified)
					MainWindow.Iconify ();
			}
			else {
				configuration = new Configuration ();
			}
			
		}
		
		static void SaveConfiguration ()
		{
			MainWindow.GetPosition (out configuration.WindowX, out configuration.WindowY);
			MainWindow.GetSize (out configuration.WindowWidth, out configuration.WindowHeight);
			configuration.WindowState = MainWindow.GdkWindow.State; 
			
			string file = Path.Combine (SteticMain.ConfigDir, "configuration.xml");
			try {
				if (!Directory.Exists (SteticMain.ConfigDir))
					Directory.CreateDirectory (SteticMain.ConfigDir);

				using (StreamWriter sw = new StreamWriter (file)) {
					XmlSerializer ser = new XmlSerializer (typeof (Configuration));
					ser.Serialize (sw, configuration);
				}
			} catch (Exception ex) {
				// Ignore exceptions while writing the recents file
				Console.WriteLine (ex);
			}
		}
		
		public static string ConfigDir {
			get { 
				string file = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), ".config");
				return Path.Combine (file, "stetic");
			}
		}
	}
}
