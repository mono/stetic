//
// WidgetEditSession.cs
//
// Author:
//   Lluis Sanchez Gual
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


using System;
using System.Xml;
using System.Reflection;
using System.Collections;
using System.CodeDom;
using Mono.Unix;

namespace Stetic {
    
	internal class WidgetEditSession: MarshalByRefObject, IDisposable
	{
		string sourceWidget;
		Stetic.ProjectBackend sourceProject;
		
		Stetic.ProjectBackend gproject;
		Stetic.Wrapper.Container rootWidget;
		Stetic.WidgetDesignerBackend widget;
		Gtk.VBox designer;
		Gtk.Plug plug;
		bool autoCommitChanges;
		WidgetActionBar toolbar;
		WidgetDesignerFrontend frontend;
		bool allowBinding;
		bool disposed;
		
		ContainerUndoRedoManager undoManager;
		UndoQueue undoQueue;
		
		public event EventHandler ModifiedChanged;
		public event EventHandler RootWidgetChanged;
		
		public WidgetEditSession (WidgetDesignerFrontend frontend, Stetic.Wrapper.Container win, Stetic.ProjectBackend editingBackend, bool autoCommitChanges)
		{
			this.frontend = frontend;
			this.autoCommitChanges = autoCommitChanges;
			undoManager = new ContainerUndoRedoManager ();
			undoQueue = new UndoQueue ();
			undoManager.UndoQueue = undoQueue;
			
			sourceWidget = win.Wrapped.Name;
			sourceProject = (ProjectBackend) win.Project;
			gproject = editingBackend;
			
			if (!autoCommitChanges) {
				// Reuse the action groups and icon factory of the main project
				gproject = editingBackend;
				gproject.ActionGroups = win.Project.ActionGroups;
				gproject.IconFactory = win.Project.IconFactory;
				gproject.ResourceProvider = win.Project.ResourceProvider;
				gproject.WidgetLibraries = (ArrayList) ((ProjectBackend)win.Project).WidgetLibraries.Clone ();
				
				rootWidget = editingBackend.GetTopLevelWrapper (sourceWidget, false);
				if (rootWidget == null) {
					// Copy the widget to edit from the source project
					// When saving the file, this project will be merged with the main project.
					XmlElement data = Stetic.WidgetUtils.ExportWidget (win.Wrapped);
					Gtk.Widget w = Stetic.WidgetUtils.ImportWidget (gproject, data);
					gproject.AddWidget (w);
					rootWidget = Stetic.Wrapper.Container.Lookup (w);
				}
				
				gproject.Modified = false;
			}
			else {
				gproject = (Stetic.ProjectBackend) win.Project;
				rootWidget = win;
			}
			
			rootWidget.Select ();
			undoManager.RootObject = rootWidget;
			
			gproject.ModifiedChanged += new EventHandler (OnModifiedChanged);
			gproject.Changed += new EventHandler (OnChanged);
			gproject.ProjectReloaded += new EventHandler (OnProjectReloaded);
//			gproject.WidgetMemberNameChanged += new Stetic.Wrapper.WidgetNameChangedHandler (OnWidgetNameChanged);
		}
		
		public bool AllowWidgetBinding {
			get { return allowBinding; }
			set {
				allowBinding = value;
				if (toolbar != null)
					toolbar.AllowWidgetBinding = allowBinding;
			}
		}
		
		public Stetic.Wrapper.Widget GladeWidget {
			get { return rootWidget; }
		}
		
		public Stetic.Wrapper.Container RootWidget {
			get { return (Wrapper.Container) Component.GetSafeReference (rootWidget); }
		}
		
		public Gtk.Widget WrapperWidget {
			get {
				if (designer == null) {
					if (rootWidget == null)
						return widget;
					Gtk.Container wc = rootWidget.Wrapped as Gtk.Container;
					if (widget == null)
						widget = Stetic.UserInterface.CreateWidgetDesigner (wc, rootWidget.DesignWidth, rootWidget.DesignHeight);
					
					toolbar = new WidgetActionBar (frontend, rootWidget);
					toolbar.AllowWidgetBinding = allowBinding;
					designer = new Gtk.VBox ();
					designer.BorderWidth = 3;
					designer.PackStart (toolbar, false, false, 0);
					designer.PackStart (widget, true, true, 3);
					widget.DesignArea.SetSelection (gproject.Selection, gproject.Selection, false);
					widget.SelectionChanged += OnSelectionChanged;
				
				}
				return designer; 
			}
		}
		
		[NoGuiDispatch]
		public void CreateWrapperWidgetPlug (uint socketId)
		{
			Gdk.Threads.Enter ();
			plug = new Gtk.Plug (socketId);
			plug.Add (WrapperWidget);
			plug.Decorated = false;
			plug.ShowAll ();
			Gdk.Threads.Leave ();
		}
		
		public void DestroyWrapperWidgetPlug ()
		{
			if (designer != null) {
				Gtk.Plug plug = (Gtk.Plug) WrapperWidget.Parent;
				plug.Remove (WrapperWidget);
				plug.Destroy ();
			}
		}
		
		public void Save ()
		{
			if (!autoCommitChanges) {
				XmlElement data = Stetic.WidgetUtils.ExportWidget (rootWidget.Wrapped);
				
				Wrapper.Widget sw = sourceProject.GetTopLevelWrapper (sourceWidget, false);
				sw.Read (new ObjectReader (gproject, FileFormat.Native), data);
				
				sourceWidget = ((Gtk.Widget)sw.Wrapped).Name;
				sw.NotifyChanged ();
				gproject.Modified = false;
			}
		}
		
		public ProjectBackend EditingBackend {
			get { return gproject; }
		}
		
		public void Dispose ()
		{
			if (toolbar != null)
				toolbar.Destroy ();
			
			gproject.ProjectReloaded -= new EventHandler (OnProjectReloaded);
//			gproject.WidgetMemberNameChanged -= new Stetic.Wrapper.WidgetNameChangedHandler (OnWidgetNameChanged);
			
			if (!autoCommitChanges) {
				// The global action group is being managed by the real stetic project,
				// so we need to remove it from the project copy before disposing it.
				gproject.ActionGroups = null;
				gproject.Dispose ();
				if (widget != null) {
					widget.SelectionChanged -= OnSelectionChanged;
					widget.Dispose ();
					widget.Destroy ();
					widget = null;
				}
			}
			if (plug != null)
				plug.Destroy ();
			gproject = null;
			rootWidget = null;
			frontend = null;
			System.Runtime.Remoting.RemotingServices.Disconnect (this);
			disposed = true;
		}
		
		public bool Disposed {
			get { return disposed; }
		}

		public override object InitializeLifetimeService ()
		{
			// Will be disconnected when calling Dispose
			return null;
		}
		
		public void SetDesignerActive ()
		{
			widget.UpdateObjectViewers ();
		}
		
		public bool Modified {
			get { return gproject.Modified; }
		}
		
		public UndoQueue UndoQueue {
			get { 
				if (undoQueue != null)
					return undoQueue;
				else
					return UndoQueue.Empty;
			}
		}
		
		void OnModifiedChanged (object s, EventArgs a)
		{
			if (frontend != null)
				frontend.NotifyModifiedChanged ();
		}
		
		void OnChanged (object s, EventArgs a)
		{
			if (frontend != null)
				frontend.NotifyChanged ();
		}
		
		void OnProjectReloaded (object s, EventArgs a)
		{
			Gtk.Widget[] tops = gproject.Toplevels;
			if (tops.Length > 0) {
				rootWidget = Stetic.Wrapper.Container.Lookup (tops[0]);
				undoManager.RootObject = rootWidget;
				if (rootWidget != null) {
					WidgetDesignerBackend oldWidget = widget;
					if (widget != null) {
						widget.SelectionChanged -= OnSelectionChanged;
						widget = null;
					}
					OnRootWidgetChanged ();
					if (oldWidget != null)
						oldWidget.Destroy ();
					return;
				}
			}
			SetErrorMode ();
		}
		
		void SetErrorMode ()
		{
			Gtk.Label lab = new Gtk.Label ();
			lab.Markup = "<b>" + Catalog.GetString ("The form designer could not be loaded") + "</b>";
			Gtk.EventBox box = new Gtk.EventBox ();
			box.Add (lab);
			
			widget = Stetic.UserInterface.CreateWidgetDesigner (box, 100, 100);
			rootWidget = null;
			
			OnRootWidgetChanged ();
		}
		
		void OnRootWidgetChanged ()
		{
			if (designer != null) {
				if (designer.Parent is Gtk.Plug)
					((Gtk.Plug)designer.Parent).Remove (designer);
				designer.Dispose ();
				designer = null;
			}
			
			if (plug != null) {
				Gdk.Threads.Enter ();
				plug.Add (WrapperWidget);
				plug.ShowAll ();
				Gdk.Threads.Leave ();
			}
			
			if (frontend != null)
				frontend.NotifyRootWidgetChanged ();
			if (RootWidgetChanged != null)
				RootWidgetChanged (this, EventArgs.Empty);
		}
		
		void OnSelectionChanged (object ob, EventArgs a)
		{
			if (frontend != null)
				frontend.NotifySelectionChanged (Component.GetSafeReference (ObjectWrapper.Lookup (widget.Selection)));
		}
		
		public object SaveState ()
		{
			return null;
		}
		
		public void RestoreState (object sessionData)
		{
		}
	}
}
