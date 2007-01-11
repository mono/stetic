
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
		
		ProjectBackend activeProject;
		static ApplicationBackendController controller;
		
		public ApplicationBackend ()
		{
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
			
/*			Gtk.Window w = new Gtk.Window ("fd");
			w.Add (new Gtk.Button("hi"));
			w.ShowAll ();
*/
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
		
		public ProjectBackend LoadProject (string path)
		{
			ProjectBackend p = new ProjectBackend ();
			
			if (System.IO.Path.GetExtension (path) == ".glade") {
				GladeFiles.Import (p, path);
			} else {
				p.Load (path);
			}
			return p;
		}
		
		public SteticCompilationUnit[] GenerateProjectCode (GenerationOptions options, ProjectBackend[] projects)
		{
			return CodeGenerator.GenerateProjectCode (options, projects);
		}
		
		public ArrayList GlobalWidgetLibraries {
			get { return globalWidgetLibraries; }
			set { globalWidgetLibraries = value; }
		}
		
		public bool UpdateLibraries (ArrayList libraries, bool allowBackendRestart, bool forceUnload)
		{
			libraries.Add (Registry.CoreWidgetLibrary.Name);
			
			if (!Registry.ReloadWidgetLibraries () && allowBackendRestart)
				return false;
			
			bool updated = false;
			
			// Check which libraries need to be unloaded
			
			foreach (WidgetLibrary alib in Registry.RegisteredWidgetLibraries) {
				if (!libraries.Contains (alib.Name)) {
					if (forceUnload && allowBackendRestart)
						return false;
					Registry.UnregisterWidgetLibrary (alib);
					updated = true;
				}
			}
			
			// Load new libraries
			
			foreach (string s in libraries) {
				if (Registry.IsRegistered (s))
					continue;
					
				// Try loading the library using the resolved delegate
				WidgetLibrary alib = null;
				if (widgetLibraryResolver != null)
					alib = widgetLibraryResolver (s);
				if (alib == null) {
					try {
						alib = new AssemblyWidgetLibrary (s);
					} catch {
						// FIXME: handle the error, but keep loading.
					}
				}
				if (alib != null) {
					try {
						Registry.RegisterWidgetLibrary (alib);
					} catch (Exception ex) {
						// Catch errors when loading a library to avoid aborting
						// the whole update method. After all, that's not a fatal
						// error (some widgets just won't be shown).
						// FIXME: return the error somewhere
						Console.WriteLine (ex);
					}
					updated = true;
				}
			}
			
			// Update the palette
			if (updated && paletteWidget != null)
				paletteWidget.WidgetLibraries = GetActiveLibraries ();
			
			return true;
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
				projectLibs.AddRange (activeProject.WidgetLibraries.GetWidgetLibraries ());
				
			ArrayList list = new ArrayList ();
			foreach (WidgetLibrary lib in Registry.RegisteredWidgetLibraries) {
				WidgetLibrary alib = lib as WidgetLibrary;
				if (alib == null) continue;
				
				string aname = alib.Name;
				if (projectLibs.Contains (aname) || globalWidgetLibraries.Contains (aname))
					list.Add (lib);
			}
			return (WidgetLibrary[]) list.ToArray (typeof(WidgetLibrary));
				
		}
		
		public ProjectBackend CreateProject ()
		{
			return new ProjectBackend ();
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
				paletteWidget = new PaletteBackend ();
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
