
using System;
using System.IO;
using System.Collections;
using System.CodeDom;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Mono.Remoting.Channels.Unix;

namespace Stetic
{
	internal class ApplicationBackend: MarshalByRefObject, IDisposable, IObjectViewer
	{
		PaletteBackend paletteWidget;
		WidgetPropertyTreeBackend propertiesWidget;
		SignalsEditorEditSession signalsWidget;
		ProjectViewBackend projectWidget;
		ArrayList globalWidgetLibraries = new ArrayList ();
		WidgetLibraryResolveHandler widgetLibraryResolver;
		object targetViewerObject;
		bool allowInProcLibraries = true;
		
		ProjectBackend activeProject;
		static ApplicationBackendController controller;
		
		static ApplicationBackend ()
		{
		}
		
		public ApplicationBackend ()
		{
			Registry.Initialize (new AssemblyWidgetLibrary (typeof(Registry).Assembly.FullName, typeof(Registry).Assembly));
			WidgetDesignerBackend.DefaultObjectViewer = this;
		}
		
		public static void Main ()
		{
			Gdk.Threads.Init ();
			Gtk.Application.Init ();
			
			System.Threading.Thread.CurrentThread.Name = "gui-thread";
			
			new Gnome.Program ("SteticBackend", "0.0", Gnome.Modules.UI, new string[0]);
			
			// Read the remoting channel to use
			
			string channel = Console.In.ReadLine ();
			
			IServerChannelSinkProvider formatterSink = new BinaryServerFormatterSinkProvider ();
			formatterSink.Next = new GuiDispatchServerSinkProvider ();
				
			string unixPath = null;
			if (channel == "unix") {
				unixPath = System.IO.Path.GetTempFileName ();
				Hashtable props = new Hashtable ();
				props ["path"] = unixPath;
				props ["name"] = "__internal_unix";
				ChannelServices.RegisterChannel (new UnixChannel (props, null, formatterSink));
			} else {
				Hashtable props = new Hashtable ();
				props ["port"] = 0;
				props ["name"] = "__internal_tcp";
				ChannelServices.RegisterChannel (new TcpChannel (props, null, formatterSink));
			}
			
			// Read the reference to the application
			
			string sref = Console.In.ReadLine ();
			byte[] data = Convert.FromBase64String (sref);
			MemoryStream ms = new MemoryStream (data);
			BinaryFormatter bf = new BinaryFormatter ();
			
			controller = (ApplicationBackendController) bf.Deserialize (ms);
			ApplicationBackend backend = new ApplicationBackend ();
			
			controller.Connect (backend);
			
			Gdk.Threads.Enter ();
			Gtk.Application.Run ();
			Gdk.Threads.Leave ();
			
			controller.Disconnect (backend);
		}
		
		public virtual void Dispose ()
		{
			if (controller != null) {
				Gtk.Application.Quit ();
			}
			System.Runtime.Remoting.RemotingServices.Disconnect (this);
		}

		public override object InitializeLifetimeService ()
		{
			// Will be disconnected when calling Dispose
			return null;
		}
		
		public WidgetLibraryResolveHandler WidgetLibraryResolver {
			get { return widgetLibraryResolver; }
			set { widgetLibraryResolver = value; }
		}
		
		public bool ShowNonContainerWarning {
			get { return Wrapper.Container.ShowNonContainerWarning; }
			set { Wrapper.Container.ShowNonContainerWarning = value; }
		}
		
		public ProjectBackend LoadProject (string path)
		{
			ProjectBackend p = new ProjectBackend (this);
			
			if (System.IO.Path.GetExtension (path) == ".glade") {
				GladeFiles.Import (p, path);
			} else {
				p.Load (path);
			}
			return p;
		}
		
		public CodeGenerationResult GenerateProjectCode (GenerationOptions options, ProjectBackend[] projects)
		{
			return CodeGenerator.GenerateProjectCode (options, projects);
		}
		
		public ArrayList GlobalWidgetLibraries {
			get { return globalWidgetLibraries; }
			set { globalWidgetLibraries = value; }
		}
		
		public bool AllowInProcLibraries {
			get { return allowInProcLibraries; }
			set { allowInProcLibraries = value; }
		}
		
		public bool UpdateLibraries (ArrayList libraries, ArrayList projects, bool allowBackendRestart, bool forceUnload)
		{
			try {
				Registry.BeginChangeSet ();
				
				libraries.Add (Registry.CoreWidgetLibrary.Name);
				
				if (!Registry.ReloadWidgetLibraries () && allowBackendRestart)
					return false;
				
				// Store a list of registered libraries, used later to know which
				// ones need to be unloaded
				
				ArrayList loaded = new ArrayList ();
				foreach (WidgetLibrary alib in Registry.RegisteredWidgetLibraries)
					loaded.Add (alib.Name);
				
				// Load required libraries
				
				Hashtable visited = new Hashtable ();
				LoadLibraries (null, visited, libraries);
				
				foreach (ProjectBackend project in projects)
					LoadLibraries (project.ImportContext, visited, project.WidgetLibraries);
				
				// Unload libraries which are not required
				
				foreach (string name in loaded) {
					if (!visited.Contains (name)) {
						if (forceUnload && allowBackendRestart)
							return false;
						Registry.UnregisterWidgetLibrary (Registry.GetWidgetLibrary (name));
					}
				}
				
				return true;
			}
			finally {
				Registry.EndChangeSet ();
			}
		}
		
		internal void LoadLibraries (ImportContext ctx, IEnumerable libraries)
		{
			try {
				Registry.BeginChangeSet ();
				Hashtable visited = new Hashtable ();
				LoadLibraries (ctx, visited, libraries);
			} finally {
				Registry.EndChangeSet ();
			}
		}
		
		internal void LoadLibraries (ImportContext ctx, Hashtable visited, IEnumerable libraries)
		{
			foreach (string s in libraries)
				AddLibrary (ctx, visited, s);
		}
		
		WidgetLibrary AddLibrary (ImportContext ctx, Hashtable visited, string s)
		{
			if (Registry.IsRegistered (s)) {
				WidgetLibrary lib = Registry.GetWidgetLibrary (s);
				CheckDependencies (ctx, visited, lib);
				return lib;
			}

			WidgetLibrary alib = CreateLibrary (ctx, s);
			if (alib == null)
				return null;
				
			RegisterLibrary (ctx, visited, alib);
			return alib;
		}
		
		void RegisterLibrary (ImportContext ctx, Hashtable visited, WidgetLibrary lib)
		{
			if (visited.Contains (lib.Name))
				return;
				
			visited [lib.Name] = lib;

			foreach (string s in lib.GetLibraryDependencies ()) {
				if (!Application.InternalIsWidgetLibrary (ctx, s))
					continue;
				AddLibrary (ctx, visited, s);
			}
			
			try {
				Registry.RegisterWidgetLibrary (lib);
			} catch (Exception ex) {
				// Catch errors when loading a library to avoid aborting
				// the whole update method. After all, that's not a fatal
				// error (some widgets just won't be shown).
				// FIXME: return the error somewhere
				Console.WriteLine (ex);
			}
		}
		
		WidgetLibrary CreateLibrary (ImportContext ctx, string name)
		{
			// Try loading the library using the resolved delegate
			WidgetLibrary alib = null;
			if (widgetLibraryResolver != null)
				alib = widgetLibraryResolver (name);
			if (alib == null) {
				try {
					if (allowInProcLibraries)
						return new AssemblyWidgetLibrary (ctx, name);
					else
						return new CecilWidgetLibrary (ctx, name);
				} catch (Exception ex) {
					// FIXME: handle the error, but keep loading.
					Console.WriteLine (ex);
				}
			}
			return null;
		}
		
		void CheckDependencies (ImportContext ctx, Hashtable visited, WidgetLibrary lib)
		{
			if (visited.Contains (lib.Name))
				return;
				
			visited [lib.Name] = lib;
			
			foreach (string dep in lib.GetLibraryDependencies ()) {
				WidgetLibrary depLib = Registry.GetWidgetLibrary (dep);
				if (depLib == null)
					AddLibrary (ctx, visited, dep);
				else
					CheckDependencies (ctx, visited, depLib);
			}
		}
		
		public ProjectBackend ActiveProject {
			get { return activeProject; }
			set {
				activeProject = value;
				if (paletteWidget != null) {
					paletteWidget.ProjectBackend = activeProject;
					paletteWidget.WidgetLibraries = GetActiveLibraries ();
				}
				if (propertiesWidget != null)
					propertiesWidget.ProjectBackend = activeProject;
				if (signalsWidget != null)
					signalsWidget.ProjectBackend = activeProject;
				if (projectWidget != null)
					projectWidget.ProjectBackend = activeProject;
			}
		}
		
		public WidgetLibrary[] GetActiveLibraries ()
		{
			ArrayList projectLibs = new ArrayList ();
			if (activeProject != null)
				projectLibs.AddRange (activeProject.WidgetLibraries);
				
			ArrayList list = new ArrayList ();
			foreach (WidgetLibrary alib in Registry.RegisteredWidgetLibraries) {
				string aname = alib.Name;
				if (projectLibs.Contains (aname) || globalWidgetLibraries.Contains (aname))
					list.Add (alib);
			}
			return (WidgetLibrary[]) list.ToArray (typeof(WidgetLibrary));
		}
		
		public ProjectBackend CreateProject ()
		{
			return new ProjectBackend (this);
		}
		
		object IObjectViewer.TargetObject {
			get { return targetViewerObject; }
			set {
				targetViewerObject = value;
				if (propertiesWidget != null)
					propertiesWidget.TargetObject = targetViewerObject;
				if (signalsWidget != null)
					signalsWidget.Editor.TargetObject = targetViewerObject;
			}
		}
		
		public PaletteBackend GetPaletteWidget ()
		{
			if (paletteWidget == null) {
				paletteWidget = new PaletteBackend (this);
				paletteWidget.ProjectBackend = activeProject;
				paletteWidget.WidgetLibraries = GetActiveLibraries ();
			}
			return paletteWidget;
		}
		
		public void CreatePaletteWidgetPlug (uint socketId)
		{
			Gtk.Plug plug = new Gtk.Plug (socketId);
			plug.Decorated = false;
//			Gtk.Window plug = new Gtk.Window ("");
			plug.Add (GetPaletteWidget ());
			plug.Show ();
		}
		
		public void DestroyPaletteWidgetPlug ()
		{
			if (paletteWidget != null) {
				Gtk.Plug plug = (Gtk.Plug)paletteWidget.Parent;
				plug.Remove (paletteWidget);
				plug.Destroy ();
			}
		}
		
		public WidgetPropertyTreeBackend GetPropertiesWidget ()
		{
			if (propertiesWidget == null) {
				propertiesWidget = new WidgetPropertyTreeBackend ();
				propertiesWidget.ProjectBackend = activeProject;
				propertiesWidget.TargetObject = targetViewerObject;
			}
			return propertiesWidget;
		}
		
		public void CreatePropertiesWidgetPlug (uint socketId)
		{
			Gtk.Plug plug = new Gtk.Plug (socketId);
			plug.Decorated = false;
//			Gtk.Window plug = new Gtk.Window ("");
			plug.Add (GetPropertiesWidget ());
			plug.Show ();
		}
		
		public void DestroyPropertiesWidgetPlug ()
		{
			if (propertiesWidget != null) {
				Gtk.Plug plug = (Gtk.Plug) propertiesWidget.Parent;
				plug.Remove (propertiesWidget);
				plug.Destroy ();
			}
		}
		
		public SignalsEditorEditSession GetSignalsWidget (SignalsEditorFrontend frontend)
		{
			if (signalsWidget == null) {
				signalsWidget = new SignalsEditorEditSession (frontend);
				signalsWidget.ProjectBackend = activeProject;
				signalsWidget.Editor.TargetObject = targetViewerObject;
			}
			return signalsWidget;
		}
		
		public SignalsEditorEditSession CreateSignalsWidgetPlug (SignalsEditorFrontend frontend, uint socketId)
		{
			Gtk.Plug plug = new Gtk.Plug (socketId);
			plug.Decorated = false;
//			Gtk.Window plug = new Gtk.Window ("");
			SignalsEditorEditSession session = GetSignalsWidget (frontend);
			plug.Add (session.Editor);
			plug.Show ();
			return session;
		}
		
		public void DestroySignalsWidgetPlug ()
		{
			if (signalsWidget != null) {
				Gtk.Plug plug = (Gtk.Plug) signalsWidget.Editor.Parent;
				plug.Remove (signalsWidget.Editor);
				plug.Destroy ();
			}
		}
		
		public ProjectViewBackend GetProjectWidget (ProjectViewFrontend frontend)
		{
			if (projectWidget == null) {
				projectWidget = new ProjectViewBackend (frontend);
				projectWidget.ProjectBackend = activeProject;
			}
			return projectWidget;
		}
		
		public void CreateProjectWidgetPlug (ProjectViewFrontend frontend, uint socketId)
		{
			Gtk.Plug plug = new Gtk.Plug (socketId);
			plug.Decorated = false;
//			Gtk.Window plug = new Gtk.Window ("");
			plug.Add (GetProjectWidget (frontend));
			plug.Show ();
		}
		
		public void DestroyProjectWidgetPlug ()
		{
			if (projectWidget != null) {
				Gtk.Plug plug = (Gtk.Plug)projectWidget.Parent;
				plug.Remove (projectWidget);
				plug.Destroy ();
			}
		}
		
		public ArrayList GetComponentTypes ()
		{
			ArrayList list = new ArrayList ();
			foreach (ClassDescriptor cd in Registry.AllClasses)
				list.Add (cd.Name);
			return list;
		}
		
		public bool GetClassDescriptorInfo (string name, out string desc, out string className, out byte[] icon)
		{
			ClassDescriptor cls = Registry.LookupClassByName (name);
			if (cls == null) {
				icon = null;
				desc = null;
				className = null;
				return false;
			}
			desc = cls.Label;
			className = cls.WrappedTypeName;
			icon = cls.Icon != null ? cls.Icon.SaveToBuffer ("png") : null;
			return true;
		}
		
		public object[] GetClassDescriptorInitializationValues (string name)
		{
			ClassDescriptor cls = Registry.LookupClassByName (name);
			ArrayList list = new ArrayList ();
			
			foreach (Stetic.PropertyDescriptor prop in cls.InitializationProperties)
			{
				if (prop.PropertyType.IsValueType) {
					// Avoid sending to the main process types which should not be loaded there
					if (prop.PropertyType.Assembly == typeof(object).Assembly ||
					    prop.PropertyType.Assembly == typeof(Gtk.Widget).Assembly ||
					    prop.PropertyType.Assembly == typeof(Gdk.Window).Assembly ||
					    prop.PropertyType.Assembly == typeof(GLib.Object).Assembly) {
					    
						list.Add (Activator.CreateInstance (prop.PropertyType));
					} else
						return new object [0];
				} else
					list.Add (null);
			}
			return list.ToArray ();
		}
		
		public void ShowPaletteGroup (string name, string label)
		{
			GetPaletteWidget ().ShowGroup (name, label);
		}
		
		public void HidePaletteGroup (string name)
		{
			GetPaletteWidget ().HideGroup (name);
		}
		
		internal void GetClipboardOperations (object obj, out bool canCut, out bool canCopy, out bool canPaste)
		{
			Stetic.Wrapper.Widget wrapper = obj as Stetic.Wrapper.Widget;
			if (wrapper != null) {
				canCut = wrapper.InternalChildProperty == null;
				canCopy = true;
				canPaste = false;
			}
			else if (obj is Placeholder) {
				// FIXME: make it work for placeholders
				canCut = canCopy = false;
				canPaste = true;
			}
			else {
				canCut = canCopy = canPaste = false;
			}
		}
		
		internal void GetComponentInfo (object obj, out string name, out string type)
		{
			Stetic.Wrapper.Widget wrapper = obj as Stetic.Wrapper.Widget;
			name = wrapper.Wrapped.Name;
			type = wrapper.ClassDescriptor.Name;
		}
		
		internal void RenameWidget (Wrapper.Widget w, string newName)
		{
			w.Wrapped.Name = newName;
		}
		
		internal byte[] GetActionIcon (Wrapper.Action ac)
		{
			Gdk.Pixbuf pix = ac.RenderIcon (Gtk.IconSize.Menu);
			return pix.SaveToBuffer ("png");
		}
		
		internal ArrayList GetWidgetChildren (Wrapper.Widget ww)
		{
			Stetic.Wrapper.Container cw = ww as Stetic.Wrapper.Container;
			if (cw == null)
				return null;
				
			ArrayList list = new ArrayList ();
			foreach (object ob in cw.RealChildren) {
				ObjectWrapper ow = ObjectWrapper.Lookup (ob);
				if (ow != null)
					list.Add (Component.GetSafeReference (ow));
			}
			return list;
		}
		
		internal void RemoveWidgetSignal (ObjectWrapper wrapper, Signal signal)
		{
			foreach (Signal s in wrapper.Signals) {
				if (s.Handler == signal.Handler && s.SignalDescriptor.Name == signal.SignalDescriptor.Name) {
					wrapper.Signals.Remove (s);
					return;
				}
			}
		}
		
		internal Wrapper.ActionGroup[] GetActionGroups (Wrapper.Widget widget)
		{
			return widget.LocalActionGroups.ToArray ();
		}
		
		internal ObjectBindInfo[] GetBoundComponents (ObjectWrapper wrapper)
		{
			return CodeGenerator.GetFieldsToBind (wrapper).ToArray ();
		}
		
		internal ObjectWrapper GetPropertyTreeTarget ()
		{
			return targetViewerObject as ObjectWrapper;
		}
	}
}
