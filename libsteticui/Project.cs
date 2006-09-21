
using System;
using System.Xml;
using System.IO;
using System.Collections;
using System.Collections.Specialized;

namespace Stetic
{
	public class Project: MarshalByRefObject
	{
		Application app;
		ProjectBackend backend;
		string fileName;
		Stetic.ProjectIconFactory iconFactory;
		IResourceProvider resourceProvider;
		Component selection;
		string tmpProjectFile;
		WidgetLibrarySet widgetLibraries = new WidgetLibrarySet ();
		
		internal event EventHandler BackendChanging;
		internal event EventHandler BackendChanged;

		public event ComponentEventHandler ComponentAdded;
		public event ComponentEventHandler ComponentRemoved;
		public event ComponentEventHandler SelectionChanged;
		public event ComponentNameEventHandler ComponentNameChanged;
		
		public event EventHandler ActionGroupsChanged;
		public event EventHandler ModifiedChanged;
		
		public event ComponentSignalEventHandler SignalAdded;
		public event ComponentSignalEventHandler SignalRemoved;
		public event ComponentSignalEventHandler SignalChanged;

		public event EventHandler ProjectReloaded;

		internal Project (Application app): this (app, app.Backend.CreateProject ())
		{
		}
		
		internal Project (Application app, ProjectBackend backend)
		{
			this.app = app;
			this.backend = backend;
			backend.SetFrontend (this);
			backend.WidgetLibraries = widgetLibraries;

			app.BackendChanging += OnBackendChanging;
			app.BackendChanged += OnBackendChanged;
		}
		
		internal ProjectBackend ProjectBackend {
			get { return backend; }
		}
		
		internal Application App {
			get { return app; }
		}
		
		public void Dispose ()
		{
			if (tmpProjectFile != null && File.Exists (tmpProjectFile)) {
				File.Delete (tmpProjectFile);
				tmpProjectFile = null;
			}
			backend.Dispose ();
			app.DisposeProject (this);
			System.Runtime.Remoting.RemotingServices.Disconnect (this);
		}

		public override object InitializeLifetimeService ()
		{
			// Will be disconnected when calling Dispose
			return null;
		}

		public string FileName {
			get { return fileName; }
		}
		
		public IResourceProvider ResourceProvider { 
			get { return resourceProvider; }
			set { 
				resourceProvider = value;
				backend.ResourceProvider = value;
			}
		}
		
		public Stetic.ProjectIconFactory IconFactory {
			get { return iconFactory; }
			set { 
				iconFactory = value;
				backend.IconFactory = value;
			}
		}
		
		public Component Selection {
			get { return selection; }
		}
		
		public void Close ()
		{
			backend.Close ();
		}
		
		public void Load (string fileName)
		{
			this.fileName = fileName;
			backend.Load (fileName);
		}
		
		public void Save (string fileName)
		{
			this.fileName = fileName;
			backend.Save (fileName);
		}
		
		public void ImportGlade (string fileName)
		{
			backend.ImportGlade (fileName);
		}
		
		public void ExportGlade (string fileName)
		{
			backend.ExportGlade (fileName);
		}
		
		public bool Modified {
			get { return backend.Modified; }
			set { backend.Modified = value;	}
		}
		
		public WidgetDesigner CreateWidgetDesigner (Component component, bool autoCommitChanges)
		{
			return new WidgetDesigner (this, component.Name, autoCommitChanges);
		}
		
		public ActionGroupDesigner CreateActionGroupDesigner (ActionGroupComponent actionGroup, bool autoCommitChanges)
		{
			return new ActionGroupDesigner (this, null, actionGroup, autoCommitChanges);
		}

		public ActionGroupDesigner CreateActionGroupDesigner (Component component, bool autoCommitChanges)
		{
			return new ActionGroupDesigner (this, component.Name, null, autoCommitChanges);
		}

		public WidgetComponent AddNewComponent (ComponentType type, string name)
		{
			object ob = backend.AddNewWidget (type.Name, name);
			return (WidgetComponent) App.GetComponent (ob, null, null);
		}
		
		public WidgetComponent AddNewComponent (XmlElement template)
		{
			object ob = backend.AddNewWidgetFromTemplate (template.OuterXml);
			return (WidgetComponent) App.GetComponent (ob, null, null);
		}
		
		public void RemoveComponent (WidgetComponent component)
		{
			backend.RemoveWidget (component.Name);
		}
		
		public WidgetComponent[] GetComponents ()
		{
			ArrayList list = new ArrayList ();
			foreach (object ob in backend.GetTopLevelWrappers ())
				list.Add (App.GetComponent (ob, null, null));
			return (WidgetComponent[]) list.ToArray (typeof(WidgetComponent));
		}
		
		public WidgetComponent GetComponent (string name)
		{
			object ob = backend.GetTopLevelWrapper (name, false);
			if (ob != null)
				return (WidgetComponent) App.GetComponent (ob, name, null);
			else
				return null;
		}
		
		public ActionGroupComponent AddNewActionGroup (string id)
		{
			object ob = backend.AddNewActionGroup (id);
			return (ActionGroupComponent) App.GetComponent (ob, id, null);
		}
		
		public ActionGroupComponent AddNewActionGroup (XmlElement template)
		{
			object ob = backend.AddNewActionGroupFromTemplate (template.OuterXml);
			return (ActionGroupComponent) App.GetComponent (ob, null, null);
		}
		
		public void RemoveActionGroup (ActionGroupComponent group)
		{
			backend.RemoveActionGroup ((Stetic.Wrapper.ActionGroup) group.Backend);
		}
		
		public ActionGroupComponent[] GetActionGroups ()
		{
			Wrapper.ActionGroup[] acs = ProjectBackend.GetActionGroups ();
			
			ActionGroupComponent[] comps = new ActionGroupComponent [acs.Length];
			for (int n=0; n<acs.Length; n++)
				comps [n] = (ActionGroupComponent) App.GetComponent (acs[n], null, null);
				
			return comps;
		}
		
		public void AddWidgetLibrary (string assemblyPath)
		{
			widgetLibraries.AddWidgetLibrary (assemblyPath);
			app.UpdateWidgetLibraries (false, false);
		}
		
		public void RemoveWidgetLibrary (string assemblyPath)
		{
			widgetLibraries.RemoveWidgetLibrary (assemblyPath);
			app.UpdateWidgetLibraries (false, false);
		}
		
		public string[] GetWidgetLibraries ()
		{
			return widgetLibraries.GetWidgetLibraries ();
		}
		
		public bool CanCopySelection {
			get { return Selection != null ? Selection.CanCopy : false; }
		}
		
		public bool CanCutSelection {
			get { return Selection != null ? Selection.CanCut : false; }
		}
		
		public bool CanPasteToSelection {
			get { return Selection != null ? Selection.CanPaste : false; }
		}
		
		public void CopySelection ()
		{
			if (Selection != null)
				backend.ClipboardCopySelection ();
		}
		
		public void CutSelection ()
		{
			if (Selection != null)
				backend.ClipboardCutSelection ();
		}
		
		public void PasteToSelection ()
		{
			if (Selection != null)
				backend.ClipboardPaste ();
		}
		
		public void DeleteSelection ()
		{
			backend.DeleteSelection ();
		}
		
		public void EditIcons ()
		{
			backend.EditIcons ();
		}		

		internal void NotifyWidgetAdded (object obj, string name, string typeName, bool topLevel)
		{
			if (topLevel) {
				Gtk.Application.Invoke (
					delegate {
						Component c = App.GetComponent (obj, name, typeName);
						if (ComponentAdded != null)
							ComponentAdded (this, new ComponentEventArgs (this, c));
					}
				);
			}
		}
		
		internal void NotifyWidgetRemoved (object obj, string name, string typeName, bool topLevel)
		{
			if (topLevel) {
				Gtk.Application.Invoke (
					delegate {
						Component c = App.GetComponent (obj, name, typeName);
						if (ComponentRemoved != null)
							ComponentRemoved (this, new ComponentEventArgs (this, c));
					}
				);
			}
		}
		
		internal void NotifySelectionChanged (object obj, string name, string typeName)
		{
			Gtk.Application.Invoke (
				delegate {
					if (obj == null)
						selection = null;
					else if (obj is Stetic.Wrapper.Widget)
						selection = App.GetComponent (obj, name, typeName);
					else
						selection = WidgetComponent.Placeholder;

					if (SelectionChanged != null)
						SelectionChanged (this, new ComponentEventArgs (this, selection));
				}
			);
		}
		
		internal void NotifyModifiedChanged ()
		{
			Gtk.Application.Invoke (
				delegate {
					if (ModifiedChanged != null)
						ModifiedChanged (this, EventArgs.Empty);
				}
			);
		}
		
		internal void NotifyWidgetNameChanged (object obj, string oldName, string newName)
		{
			Gtk.Application.Invoke (
				delegate {
					if (ComponentNameChanged != null) {
						WidgetComponent c = (WidgetComponent) App.GetComponent (obj, null, null);
						c.UpdateName (newName);
						ComponentNameChanged (this, new ComponentNameEventArgs (this, c, oldName));
					}
				}
			);
		}
		
		internal void NotifyActionGroupAdded (string group)
		{
			Gtk.Application.Invoke (delegate {
				if (ActionGroupsChanged != null)
					ActionGroupsChanged (this, EventArgs.Empty);
			});
		}
		
		internal void NotifyActionGroupRemoved (string group)
		{
			Gtk.Application.Invoke (delegate {
				if (ActionGroupsChanged != null)
					ActionGroupsChanged (this, EventArgs.Empty);
			});
		}
		
		internal void NotifySignalAdded (object obj, string name, Signal signal)
		{
			Gtk.Application.Invoke (delegate {
				if (SignalAdded != null) {
					Component c = App.GetComponent (obj, name, null);
					SignalAdded (this, new ComponentSignalEventArgs (this, c, null, signal));
				}
			});
		}
		
		internal void NotifySignalRemoved (object obj, string name, Signal signal)
		{
			Gtk.Application.Invoke (delegate {
				if (SignalRemoved != null) {
					Component c = App.GetComponent (obj, name, null);
					SignalRemoved (this, new ComponentSignalEventArgs (this, c, null, signal));
				}
			});
		}
		
		internal void NotifySignalChanged (object obj, string name, Signal oldSignal, Signal signal)
		{
			Gtk.Application.Invoke (delegate {
				if (SignalChanged != null) {
					Component c = App.GetComponent (obj, name, null);
					SignalChanged (this, new ComponentSignalEventArgs (this, c, oldSignal, signal));
				}
			});
		}
		
		internal void NotifyProjectReloaded ()
		{
			Gtk.Application.Invoke (delegate {
				if (ProjectReloaded != null)
					ProjectReloaded (this, EventArgs.Empty);
			});
		}
		
		void OnBackendChanging (object ob, EventArgs args)
		{
			selection = null;
			if (SelectionChanged != null)
				SelectionChanged (this, new ComponentEventArgs (this, selection));
				
			tmpProjectFile = Path.GetTempFileName ();
			backend.Save (tmpProjectFile);
			
			if (BackendChanging != null)
				BackendChanging (this, args);
		}
		
		void OnBackendChanged (object ob, EventArgs args)
		{
			backend = app.Backend.CreateProject ();
			backend.SetFrontend (this);
			backend.WidgetLibraries = widgetLibraries;

			if (tmpProjectFile != null && File.Exists (tmpProjectFile)) {
				backend.Load (tmpProjectFile);
				backend.FileName = fileName;
				File.Delete (tmpProjectFile);
				tmpProjectFile = null;
			} else if (fileName != null) {
				backend.Load (fileName);
			}

			if (resourceProvider != null)
				backend.ResourceProvider = resourceProvider;

			if (BackendChanged != null)
				BackendChanged (this, args);
		}
	}
	
	class WidgetLibrarySet: MarshalByRefObject
	{
		ArrayList widgetLibraries = new ArrayList ();
		
		public void AddWidgetLibrary (string assemblyPath)
		{
			if (!widgetLibraries.Contains (assemblyPath)) {
				widgetLibraries.Add (assemblyPath);
			}
		}
		
		public void RemoveWidgetLibrary (string assemblyPath)
		{
			widgetLibraries.Remove (assemblyPath);
		}
		
		public string[] GetWidgetLibraries ()
		{
			return (string[]) widgetLibraries.ToArray (typeof(string));
		}
	}
}
