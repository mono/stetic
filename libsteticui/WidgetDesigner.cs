
using System;

namespace Stetic
{
	public class WidgetDesigner: PluggableWidget
	{
		WidgetEditSession session;
		WidgetDesignerFrontend frontend;
		Component selection;
		Component rootWidget;
		
		string sessionData;
		Project project;
		Project editedProject;
		int reloadCount;
		
		string componentName;
		bool autoCommitChanges;
		
		public event EventHandler BindField;
		public event EventHandler ModifiedChanged;
		public event EventHandler SelectionChanged;
		public event ComponentSignalEventHandler SignalAdded;
		public event ComponentSignalEventHandler SignalRemoved;
		public event ComponentSignalEventHandler SignalChanged;
		public event ComponentNameEventHandler ComponentNameChanged;
		
		internal WidgetDesigner (Project project, string componentName, bool autoCommitChanges): base (project.App)
		{
			this.componentName = componentName;
			this.autoCommitChanges = autoCommitChanges;
			this.project = project;
			frontend = new WidgetDesignerFrontend (this);
			
			if (autoCommitChanges)
				editedProject = project;
			else 
				editedProject = new Project (project.App);
			
			editedProject.SignalAdded += OnSignalAdded;
			editedProject.SignalRemoved += OnSignalRemoved;
			editedProject.SignalChanged += OnSignalChanged;
			editedProject.ComponentNameChanged += OnComponentNameChanged;
			
			project.BackendChanged += OnProjectBackendChanged;
			editedProject.BackendChanged += OnProjectBackendChanged;
			
			CreateSession ();
		}
		
		public Component RootComponent {
			get { return rootWidget; }
		}
		
		// Creates an action group designer for the widget being edited by this widget designer
		public ActionGroupDesigner CreateActionGroupDesigner ()
		{
			return new ActionGroupDesigner (editedProject, componentName, null, true);
		}
		
		public bool Modified {
			get { return session != null && session.Modified; }
		}
		
		public void SetActive ()
		{
			project.App.ActiveProject = editedProject;
		}
		
		public bool AllowWidgetBinding {
			get { return session != null && session.AllowWidgetBinding; }
			set {
				if (session != null)
					session.AllowWidgetBinding = value;
			}
		}
		
		void CreateSession ()
		{
			if (!app.UseExternalBackend && session != null)
				session.RootWidgetChanged -= OnRootWidgetChanged;
					
			try {
				session = project.ProjectBackend.CreateWidgetDesignerSession (frontend, componentName, editedProject.ProjectBackend, autoCommitChanges);
				if (!app.UseExternalBackend)
					session.RootWidgetChanged += OnRootWidgetChanged;
				ResetCustomWidget ();
				rootWidget = app.GetComponent (session.RootWidget, null, null);
			} catch (Exception ex) {
				Console.WriteLine (ex);
				AddCustomWidget (new Gtk.Label (ex.Message));
				session = null;
			}
		}
		
		protected override void OnCreatePlug (uint socketId)
		{
			session.CreateWrapperWidgetPlug (socketId);
		}
		
		protected override Gtk.Widget OnCreateWidget ()
		{
			return session.WrapperWidget;
		}
		
		public Component Selection {
			get {
				return selection;
			}
		}
		
		public void CopySelection ()
		{
			if (session != null)
				session.EditingBackend.ClipboardCopySelection ();
		}
		
		public void CutSelection ()
		{
			if (session != null)
				session.EditingBackend.ClipboardCutSelection ();
		}
		
		public void PasteToSelection ()
		{
			if (session != null)
				session.EditingBackend.ClipboardPaste ();
		}
		
		public void DeleteSelection ()
		{
			if (session != null)
				session.EditingBackend.DeleteSelection ();
		}
		
		public override void Dispose ()
		{
			editedProject.SignalAdded -= OnSignalAdded;
			editedProject.SignalRemoved -= OnSignalRemoved;
			editedProject.SignalChanged -= OnSignalChanged;
			editedProject.ComponentNameChanged -= OnComponentNameChanged;
			editedProject.BackendChanged -= OnProjectBackendChanged;
			project.BackendChanged -= OnProjectBackendChanged;
			
			if (!autoCommitChanges)
				editedProject.Dispose ();

			System.Runtime.Remoting.RemotingServices.Disconnect (frontend);
			if (session != null)
				session.Dispose ();
			base.Dispose ();
		}
		
		public void Save ()
		{
			if (session != null)
				session.Save ();
		}
		
		void OnComponentNameChanged (object s, ComponentNameEventArgs args)
		{
			if (ComponentNameChanged != null)
				ComponentNameChanged (this, args);
		}
		
		void OnRootWidgetChanged (object o, EventArgs a)
		{
			rootWidget = app.GetComponent (session.RootWidget, null, null);
			UpdateWidget ();
		}
		
		protected override void OnBackendChanging ()
		{
			reloadCount = 0;
			if (!autoCommitChanges)
				sessionData = session.SaveState ();
			if (session != null)
				session.Dispose ();
			session = null;
			base.OnBackendChanging ();
		}
		
		protected override void OnBackendChanged ()
		{
			// Can't do anything until the projects are reloaded
			// This is checked in OnProjectBackendChanged.
		}
		
		void OnProjectBackendChanged (object s, EventArgs a)
		{
			if (++reloadCount == 2) {
				CreateSession ();
				if (sessionData != null && session != null)
					session.RestoreState (sessionData);
				base.OnBackendChanged ();
			}
		}
		
		void OnSignalAdded (object sender, Stetic.ComponentSignalEventArgs args)
		{
			if (SignalAdded != null)
				SignalAdded (this, args);
		}

		void OnSignalRemoved (object sender, Stetic.ComponentSignalEventArgs args)
		{
			if (SignalRemoved != null)
				SignalRemoved (this, args);
		}

		void OnSignalChanged (object sender, Stetic.ComponentSignalEventArgs args)
		{
			if (SignalChanged != null)
				SignalChanged (this, args);
		}
		
		internal void NotifyBindField ()
		{
			if (BindField != null)
				BindField (this, EventArgs.Empty);
		}
		
		internal void NotifyModifiedChanged ()
		{
			if (ModifiedChanged != null)
				ModifiedChanged (this, EventArgs.Empty);
		}
		
		internal void NotifySelectionChanged (object ob)
		{
			if (ob != null)
				selection = app.GetComponent (ob, null, null);
			else
				selection = null;

			if (SelectionChanged != null)
				SelectionChanged (this, EventArgs.Empty);
		}
	}
	
	internal class WidgetDesignerFrontend: MarshalByRefObject
	{
		WidgetDesigner designer;
		
		public WidgetDesignerFrontend (WidgetDesigner designer)
		{
			this.designer = designer;
		}
		
		public void NotifyBindField ()
		{
			Gtk.Application.Invoke (
				delegate { designer.NotifyBindField (); }
			);
		}
		
		public void NotifyModifiedChanged ()
		{
			Gtk.Application.Invoke (
				delegate { designer.NotifyModifiedChanged (); }
			);
		}
		
		internal void NotifySelectionChanged (object ob)
		{
			Gtk.Application.Invoke (
				delegate { designer.NotifySelectionChanged (ob); }
			);
		}

		public override object InitializeLifetimeService ()
		{
			// Will be disconnected when calling Dispose
			return null;
		}
	}
}
