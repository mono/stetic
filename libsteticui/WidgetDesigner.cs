
using System;

namespace Stetic
{
	public class WidgetDesigner: PluggableWidget
	{
		WidgetEditSession session;
		WidgetDesignerFrontend frontend;
		Component selection;
		Component rootWidget;
		
		Project project;
		Project editedProject;
		int reloadCount;
		
		string componentName;
		bool autoCommitChanges;
		bool disposed;
		
		public event EventHandler BindField;
		public event EventHandler ModifiedChanged;
		public event EventHandler Changed;
		public event EventHandler SelectionChanged;
		public event EventHandler RootComponentChanged;
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
			return new ActionGroupDesigner (editedProject, componentName, null, this, true);
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
			try {
				session = project.ProjectBackend.CreateWidgetDesignerSession (frontend, componentName, editedProject.ProjectBackend, autoCommitChanges);
				ResetCustomWidget ();
				rootWidget = app.GetComponent (session.RootWidget, null, null);
			} catch (Exception ex) {
				Console.WriteLine (ex);
				Gtk.Label lab = new Gtk.Label ();
				lab.Text = Mono.Unix.Catalog.GetString ("The desginer could not be loaded.") + "\n\n" + ex.Message;
				lab.Wrap = true;
				lab.WidthRequest = 400;
				AddCustomWidget (lab);
				session = null;
			}
		}
		
		protected override void OnCreatePlug (uint socketId)
		{
			session.CreateWrapperWidgetPlug (socketId);
		}
		
		protected override void OnDestroyPlug (uint socketId)
		{
			session.DestroyWrapperWidgetPlug ();
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
		
		public UndoQueue UndoQueue {
			get {
				if (session != null)
					return session.UndoQueue;
				else
					return UndoQueue.Empty;
			}
		}
		
		public override void Dispose ()
		{
			if (disposed)
				return;
			
			if (project.App.ActiveProject == editedProject)
				project.App.ActiveProject = null;
			
			disposed = true;
			frontend.disposed = true;
			editedProject.SignalAdded -= OnSignalAdded;
			editedProject.SignalRemoved -= OnSignalRemoved;
			editedProject.SignalChanged -= OnSignalChanged;
			editedProject.ComponentNameChanged -= OnComponentNameChanged;
			editedProject.BackendChanged -= OnProjectBackendChanged;
			project.BackendChanged -= OnProjectBackendChanged;
			
			if (!autoCommitChanges)
				editedProject.Dispose ();

			if (session != null)
				session.Dispose ();
			base.Dispose ();

			System.Runtime.Remoting.RemotingServices.Disconnect (frontend);
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
		
		internal void NotifyRootWidgetChanged ()
		{
			object rw = session.RootWidget;
			if (rw != null)
				rootWidget = app.GetComponent (session.RootWidget, null, null);
			else
				rootWidget = null;

			UpdateWidget ();
			if (RootComponentChanged != null)
				RootComponentChanged (this, EventArgs.Empty);
		}
		
		internal override void OnBackendChanging ()
		{
			reloadCount = 0;
			base.OnBackendChanging ();
		}
		
		internal override void OnBackendChanged (ApplicationBackend oldBackend)
		{
			// Can't do anything until the projects are reloaded
			// This is checked in OnProjectBackendChanged.
		}
		
		void OnProjectBackendChanged (ApplicationBackend oldBackend)
		{
			if (++reloadCount == 2) {
				object sessionData = null;
				
				if (oldBackend != null && !autoCommitChanges) {
					sessionData = session.SaveState ();
					session.DestroyWrapperWidgetPlug ();
				}

				// Don't dispose the session here, since it will dispose
				// the underlying project, and we can't do it because
				// it may need to process the OnBackendChanging event
				// as well.
			
				CreateSession ();
				
				if (sessionData != null && session != null)
					session.RestoreState (sessionData);

				base.OnBackendChanged (oldBackend);
				NotifyRootWidgetChanged ();
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
		
		internal void NotifyChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
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
		internal bool disposed;
		
		public WidgetDesignerFrontend (WidgetDesigner designer)
		{
			this.designer = designer;
		}
		
		public void NotifyBindField ()
		{
			Gtk.Application.Invoke (
				delegate { if (!disposed) designer.NotifyBindField (); }
			);
		}
		
		public void NotifyModifiedChanged ()
		{
			Gtk.Application.Invoke (
				delegate { if (!disposed) designer.NotifyModifiedChanged (); }
			);
		}
		
		public void NotifyChanged ()
		{
			Gtk.Application.Invoke (
				delegate { if (!disposed) designer.NotifyChanged (); }
			);
		}
		
		internal void NotifySelectionChanged (object ob)
		{
			Gtk.Application.Invoke (
				delegate { if (!disposed) designer.NotifySelectionChanged (ob); }
			);
		}

		public void NotifyRootWidgetChanged ()
		{
			Gtk.Application.Invoke (
				delegate { if (!disposed) designer.NotifyRootWidgetChanged (); }
			);
		}
		
		public override object InitializeLifetimeService ()
		{
			// Will be disconnected when calling Dispose
			return null;
		}
	}
}
