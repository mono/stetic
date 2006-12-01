
using System;
using System.Threading;
using System.Collections;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Diagnostics;
using Mono.Remoting.Channels.Unix;

namespace Stetic
{
	public enum IsolationMode
	{
		None,
		ProcessTcp,
		ProcessUnix
	}
	
	public delegate WidgetLibrary WidgetLibraryResolveHandler (string name);
	
	public class Application: MarshalByRefObject, IDisposable
	{
		ApplicationBackend backend;
		bool externalBackend;
		string channelId;
		ApplicationBackendController backendController;
		
		Hashtable components = new Hashtable ();
		Hashtable types = new Hashtable ();
		ArrayList widgetLibraries = new ArrayList ();
		ArrayList projects = new ArrayList ();
		Project activeProject;
		
		WidgetPropertyTree propertiesWidget;
		Palette paletteWidget;
		ProjectView projectWidget;
		SignalsEditor signalsWidget;
		
		internal event BackendChangingHandler BackendChanging;
		internal event BackendChangedHandler BackendChanged;
		
		public Application (IsolationMode mode)
		{
			if (mode == IsolationMode.None) {
				backend = new ApplicationBackend ();
				externalBackend = false;
			} else {
				externalBackend = true;
				channelId = RegisterRemotingChannel (mode);
				backendController = new ApplicationBackendController (channelId);
				backendController.StartBackend ();
				OnBackendChanged (false);
			}
		}
		
		public WidgetLibraryResolveHandler WidgetLibraryResolver {
			get { return backend.WidgetLibraryResolver; }
			set { 
				if (UseExternalBackend)
					throw new InvalidOperationException ("Can use a custom library resolver when running in isolated mode.");
				backend.WidgetLibraryResolver = value;
			}
		}
		
		// Loads the libraries registered in the projects or in the application.
		// It will reload the libraries if they have changed. Libraries won't be
		// unloaded unless forceUnload is set to true
		public void UpdateWidgetLibraries (bool forceUnload)
		{
			UpdateWidgetLibraries (true, forceUnload);
		}
		
		internal void UpdateWidgetLibraries (bool allowBackendRestart, bool forceUnload)
		{
			// Collect libraries from the project and from the application
			
			ArrayList assemblies = new ArrayList ();
			assemblies.AddRange (widgetLibraries);
			
			foreach (Project p in projects) {
				foreach (string s in p.GetWidgetLibraries ())
					if (!assemblies.Contains (s))
						assemblies.Add (s);
			}
			
			if (!backend.UpdateLibraries (assemblies, allowBackendRestart, forceUnload))
			{
				// The backend process needs to be restarted.
				// This is done in background.
				
				ThreadPool.QueueUserWorkItem (delegate {
					try {
						// Start the new backend
						ApplicationBackendController newController = new ApplicationBackendController (channelId);
						newController.StartBackend ();
						Gtk.Application.Invoke (newController, EventArgs.Empty, OnNewBackendStarted);
					} catch {
						// FIXME: show an error message
					}
				});
			}
		}
		
		void OnNewBackendStarted (object ob, EventArgs args)
		{
			// The new backend is running, just do the switch
			
			OnBackendChanging ();
			
			ApplicationBackendController oldBackend = backendController;
			backendController = (ApplicationBackendController) ob;
			
			OnBackendChanged (true);

			// The old backend can now be safely stopped
			oldBackend.StopBackend (false);
		}
		
		void OnBackendStopped (object ob, EventArgs args)
		{
			// The backend process crashed, try to restart it
			backend = null;
			backendController = new ApplicationBackendController (channelId);
			backendController.StartBackend ();
			OnBackendChanged (true);
		}
		
		void OnBackendChanged (bool notify)
		{
			ApplicationBackend oldBackend = backend;
			
			backend = backendController.Backend;
			backendController.Stopped += OnBackendStopped;
			UpdateWidgetLibraries (false, false);
			types.Clear ();
			
			// All components should have been cleared by the backend,
			// just make sure it did
			Component[] comps = new Component [components.Count];
			components.Values.CopyTo (comps, 0);
			components.Clear ();
			
			foreach (Component c in comps) {
				c.Dispose ();
			}

			if (notify && BackendChanged != null)
				BackendChanged (oldBackend);

			backend.ActiveProject = activeProject != null ? activeProject.ProjectBackend : null;
		}
		
		void OnBackendChanging ()
		{
			backend.GlobalWidgetLibraries = widgetLibraries;
			if (BackendChanging != null)
				BackendChanging ();
		}
		
		internal string RegisterRemotingChannel (IsolationMode mode)
		{
			string remotingChannel;
			if (mode == IsolationMode.ProcessTcp) {
				remotingChannel = "tcp";
				IChannel ch = ChannelServices.GetChannel ("tcp");
				if (ch == null) {
					ChannelServices.RegisterChannel (new TcpChannel (0));
				}
			} else {
				remotingChannel = "unix";
				IChannel ch = ChannelServices.GetChannel ("unix");
				if (ch == null) {
					string unixRemotingFile = Path.GetTempFileName ();
					ChannelServices.RegisterChannel (new UnixChannel (unixRemotingFile));
				}
			}
			return remotingChannel;
		}
		
		public virtual void Dispose ()
		{
			if (externalBackend) {
				backendController.StopBackend (true);
			} else {
				backend.Dispose ();
			}
			System.Runtime.Remoting.RemotingServices.Disconnect (this);
		}

		public override object InitializeLifetimeService ()
		{
			// Will be disconnected when calling Dispose
			return null;
		}
		
		internal ApplicationBackend Backend {
			get { return backend; }
		}
		
		public Project LoadProject (string path)
		{
			Project p = new Project (this);
			p.Load (path);
			projects.Add (p);
			return p;
		}
		
		public Project CreateProject ()
		{
			Project p = new Project (this);
			projects.Add (p);
			return p;
		}
		
		public void AddWidgetLibrary (string assemblyPath)
		{
			if (!widgetLibraries.Contains (assemblyPath)) {
				widgetLibraries.Add (assemblyPath);
				backend.GlobalWidgetLibraries = widgetLibraries; 
				UpdateWidgetLibraries (false, false);
			}
		}
		
		public void RemoveWidgetLibrary (string assemblyPath)
		{
			widgetLibraries.Remove (assemblyPath);
			backend.GlobalWidgetLibraries = widgetLibraries; 
			UpdateWidgetLibraries (false, false);
		}
		
		public string[] GetWidgetLibraries ()
		{
			return (string[]) widgetLibraries.ToArray (typeof(string));
		}
		
		internal void DisposeProject (Project p)
		{
			projects.Remove (p);
		}
		
		public void GenerateProjectCode (string file, string namespaceName, CodeDomProvider provider, GenerationOptions options, params Project[] projects)
		{
			CodeCompileUnit cunit = GenerateProjectCode (namespaceName, options, projects);
			
			ICodeGenerator gen = provider.CreateGenerator ();
			StreamWriter fileStream = new StreamWriter (file);
			try {
				gen.GenerateCodeFromCompileUnit (cunit, fileStream, new CodeGeneratorOptions ());
			} finally {
				fileStream.Close ();
			}
		}
		
		public CodeCompileUnit GenerateProjectCode (string namespaceName, GenerationOptions options, params Project[] projects)
		{
			ProjectBackend[] pbs = new ProjectBackend [projects.Length];
			for (int n=0; n<projects.Length; n++)
				pbs [n] = projects [n].ProjectBackend;
				
			CodeCompileUnit cunit = new CodeCompileUnit ();
			cunit.Namespaces.Add (backend.GenerateProjectCode (namespaceName, options, pbs));
			return cunit;
		}
		
		internal bool UseExternalBackend {
			get { return externalBackend; }
		}
			
		public Project ActiveProject {
			get { return activeProject; }
			set { 
				activeProject = value;
				backend.ActiveProject = value != null ? value.ProjectBackend : null;
			}
		}
		
		public WidgetPropertyTree PropertiesWidget {
			get {
				if (propertiesWidget == null)
					propertiesWidget = new WidgetPropertyTree (this);
				return propertiesWidget;
			}
		}
		
		public Palette PaletteWidget {
			get {
				if (paletteWidget == null)
					paletteWidget = new Palette (this);
				return paletteWidget;
			}
		}
		
		public ProjectView ProjectWidget {
			get {
				if (projectWidget == null)
					projectWidget = new ProjectView (this);
				return projectWidget;
			}
		}
		
		public SignalsEditor SignalsWidget {
			get {
				if (signalsWidget == null)
					signalsWidget = new SignalsEditor (this);
				return signalsWidget;
			}
		}
		
		internal ComponentType GetComponentType (string typeName)
		{
			ComponentType t = (ComponentType) types [typeName];
			if (t != null) return t;
			
			if (typeName == "Gtk.Action" || typeName == "Gtk.ActionGroup") {
				t = new ComponentType (this, typeName, "", typeName, ComponentType.Unknown.Icon);
				types [typeName] = t;
				return t;
			}
			
			byte[] icon;
			string desc = null, className = null;
			Gdk.Pixbuf px = null;
			
			if (backend.GetClassDescriptorInfo (typeName, out desc, out className, out icon)) {
				if (icon != null)
					px = new Gdk.Pixbuf (icon);
			}
			
			if (px == null) {
				px = ComponentType.Unknown.Icon;
			}
			
			if (desc == null)
				desc = typeName;
			
			t = new ComponentType (this, typeName, desc, className, px);
			types [typeName] = t;
			return t;
		}
		
		internal Component GetComponent (object cbackend, string name, string type)
		{
			try {
				Component c = (Component) components [cbackend];
				if (c != null)
					return c;

				// If the remote object is already disposed, don't try to create a
				// local component.
				if (cbackend is ObjectWrapper && ((ObjectWrapper)cbackend).IsDisposed)
					return null;
				
				if (cbackend is Wrapper.Action) {
					c = new ActionComponent (this, cbackend, name);
					((ObjectWrapper)cbackend).Frontend = c;
				} else if (cbackend is Wrapper.ActionGroup) {
					c = new ActionGroupComponent (this, cbackend, name);
					((Wrapper.ActionGroup)cbackend).Frontend = c;
				} else if (cbackend is ObjectWrapper) {
					c = new WidgetComponent (this, cbackend, name, type != null ? GetComponentType (type) : null);
					((ObjectWrapper)cbackend).Frontend = c;
				} else if (cbackend == null)
					throw new System.ArgumentNullException ("cbackend");
				else
					throw new System.InvalidOperationException ("Invalid component type: " + cbackend.GetType ());

				components [cbackend] = c;
				return c;
			}
			catch (System.Runtime.Remoting.RemotingException ex)
			{
				// There may be a remoting exception if the remote wrapper
				// has already been disconnected when trying to create the
				// component frontend. This may happen since calls are
				// dispatched in the GUI thread, so they may be delayed.
				return null;
			}
		}
		
		internal void DisposeComponent (Component c)
		{
			components.Remove (c.Backend);
		}
	}

	internal delegate void BackendChangingHandler ();
	internal delegate void BackendChangedHandler (ApplicationBackend oldBackend);
}
