using Gtk;
using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;
using System.CodeDom;
using Mono.Unix;

namespace Stetic {

	internal class ProjectBackend : MarshalByRefObject, IProject, IDisposable 
	{
		Hashtable nodes;
		ArrayList topLevels;
		NodeStore store;
		bool modified;
		Gtk.Widget selection;
		string id;
		string fileName;
		XmlDocument tempDoc;
		bool loading;
		IResourceProvider resourceProvider;
		
		// Global action groups of the project
		Stetic.Wrapper.ActionGroupCollection actionGroups;
		bool ownedGlobalActionGroups = true;	// It may be false when reusing groups from another project
		
		Stetic.ProjectIconFactory iconFactory;
		Project frontend;
		int componentIdCounter;
		ArrayList widgetLibraries;
		ArrayList internalLibs;
		ApplicationBackend app;
		ImportContext importContext;
		
		// The action collection of the last selected widget
		Stetic.Wrapper.ActionGroupCollection oldTopActionCollection;
		
		public event Wrapper.WidgetNameChangedHandler WidgetNameChanged;
		public event Wrapper.WidgetNameChangedHandler WidgetMemberNameChanged;
		public event Wrapper.WidgetEventHandler WidgetAdded;
		public event Wrapper.WidgetEventHandler WidgetRemoved;
		public event ObjectWrapperEventHandler ObjectChanged;
		public event EventHandler ComponentTypesChanged;
		
		public event SignalEventHandler SignalAdded;
		public event SignalEventHandler SignalRemoved;
		public event SignalChangedEventHandler SignalChanged;
		
		public event Wrapper.WidgetEventHandler SelectionChanged;
		public event EventHandler ModifiedChanged;
		public event EventHandler Changed;
		
		// Fired when the project has been reloaded, due for example to
		// a change in the registry
		public event EventHandler ProjectReloaded;

		public ProjectBackend (ApplicationBackend app)
		{
			this.app = app;
			nodes = new Hashtable ();
			store = new NodeStore (typeof (ProjectNode));
			topLevels = new ArrayList ();
			
			ActionGroups = new Stetic.Wrapper.ActionGroupCollection ();

			Registry.RegistryChanging += OnRegistryChanging;
			Registry.RegistryChanged += OnRegistryChanged;
			
			iconFactory = new ProjectIconFactory ();
			widgetLibraries = new ArrayList ();
			internalLibs = new ArrayList ();
		}
		
		public void Dispose ()
		{
			// First of all, disconnect from the frontend,
			// to avoid sending notifications while disposing
			frontend = null;
			
			if (oldTopActionCollection != null)
				oldTopActionCollection.ActionGroupChanged -= OnComponentTypesChanged;

			Registry.RegistryChanging -= OnRegistryChanging;
			Registry.RegistryChanged -= OnRegistryChanged;
			Close ();
			store.Dispose ();
			iconFactory = null;
			ActionGroups = null;
			System.Runtime.Remoting.RemotingServices.Disconnect (this);
		}

		public override object InitializeLifetimeService ()
		{
			// Will be disconnected when calling Dispose
			return null;
		}
		
		public string FileName {
			get { return fileName; }
			set {
				this.fileName = value;
				if (fileName != null)
					Id = System.IO.Path.GetFileName (fileName);
				else
					Id = null;
			}
		}
		
		internal ArrayList WidgetLibraries {
			get { return widgetLibraries; }
			set { widgetLibraries = value; }
		}
		
		internal ArrayList InternalWidgetLibraries {
			get { return internalLibs; }
			set { internalLibs = value; }
		}
		
		public bool IsInternalLibrary (string lib)
		{
			return internalLibs.Contains (lib);
		}
		
		public bool CanGenerateCode {
			get {
				// It can generate code if all libraries on which depend can generate code
				foreach (string s in widgetLibraries) {
					WidgetLibrary lib = Registry.GetWidgetLibrary (s);
					if (lib != null && !lib.CanGenerateCode)
						return false;
				}
				return true;
			}
		}
		
		public void AddWidgetLibrary (string lib)
		{
			AddWidgetLibrary (lib, false);
		}
		
		public void AddWidgetLibrary (string lib, bool isInternal)
		{
			if (!widgetLibraries.Contains (lib))
				widgetLibraries.Add (lib);
			if (isInternal) {
				if (!internalLibs.Contains (lib))
					internalLibs.Add (lib);
			}
			else {
				internalLibs.Remove (lib);
			}
		}
		
		public void RemoveWidgetLibrary (string lib)
		{
			widgetLibraries.Remove (lib);
			internalLibs.Remove (lib);
		}
		
		public ArrayList GetComponentTypes ()
		{
			ArrayList list = new ArrayList ();
			foreach (WidgetLibrary lib in app.GetProjectLibraries (this)) {
				// Don't include in the list widgets which are internal (when the library is
				// not internal to the project), widgets not assigned to any category, and deprecated ones.
				bool isInternalLib = IsInternalLibrary (lib.Name);
				foreach (ClassDescriptor cd in lib.AllClasses) {
					if (!cd.Deprecated && cd.Category.Length > 0 && (isInternalLib || !cd.IsInternal))
						list.Add (cd.Name);
				}
			}
			return list;
		}
		
		public IResourceProvider ResourceProvider { 
			get { return resourceProvider; }
			set { resourceProvider = value; }
		}
		
		public Stetic.Wrapper.ActionGroupCollection ActionGroups {
			get { return actionGroups; }
			set {
				if (actionGroups != null) {
					actionGroups.ActionGroupAdded -= OnGroupAdded;
					actionGroups.ActionGroupRemoved -= OnGroupRemoved;
					actionGroups.ActionGroupChanged -= OnComponentTypesChanged;
				}
				actionGroups = value;
				if (actionGroups != null) {
					actionGroups.ActionGroupAdded += OnGroupAdded;
					actionGroups.ActionGroupRemoved += OnGroupRemoved;
					actionGroups.ActionGroupChanged += OnComponentTypesChanged;
				}
				ownedGlobalActionGroups = true;
			}
		}
		
		public void AttachActionGroups (Stetic.Wrapper.ActionGroupCollection groups)
		{
			ActionGroups = groups;
			ownedGlobalActionGroups = false;
		}
		
		public Stetic.ProjectIconFactory IconFactory {
			get { return iconFactory; }
			set { iconFactory = value; }
		}
		
		internal void SetFileName (string fileName)
		{
			this.fileName = fileName;
		}
		
		internal void SetFrontend (Project project)
		{
			frontend = project;
		}
		
		public void Close ()
		{
			fileName = null;
			
			if (actionGroups != null && ownedGlobalActionGroups) {
				foreach (Stetic.Wrapper.ActionGroup ag in actionGroups)
					ag.Dispose ();
				actionGroups.Clear ();
			}

			foreach (Gtk.Widget w in Toplevels) {
				Wrapper.Widget ww = Wrapper.Widget.Lookup (w);
				ww.Destroyed -= WidgetDestroyed;
				ProjectNode node = nodes[w] as ProjectNode;
				if (node != null)
					UnhashNodeRecursive (node);
				w.Destroy ();
			}

			selection = null;
			store.Clear ();
			nodes.Clear ();
			topLevels.Clear ();
			widgetLibraries.Clear ();

			iconFactory = new ProjectIconFactory ();
		}
		
		public void Load (string fileName)
		{
			Load (fileName, fileName);
		}
		
		public void Load (string xmlFile, string fileName)
		{
			this.fileName = fileName;
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			doc.Load (xmlFile);
			Read (doc);
			
			Id = System.IO.Path.GetFileName (fileName);
		}
		
		void Read (XmlDocument doc)
		{
			loading = true;
			string basePath = fileName != null ? Path.GetDirectoryName (fileName) : null;
			
			try {
				string fn = fileName;
				Close ();
				fileName = fn;
				
				XmlNode node = doc.SelectSingleNode ("/stetic-interface");
				if (node == null)
					throw new ApplicationException (Catalog.GetString ("Not a Stetic file according to node name."));
				
				// Load the assembly directories
				importContext = new ImportContext ();
				foreach (XmlElement libElem in node.SelectNodes ("import/assembly-directory")) {
					string dir = libElem.GetAttribute ("path");
					if (dir.Length > 0) {
						if (basePath != null && !Path.IsPathRooted (dir)) {
							dir = Path.Combine (basePath, dir);
							if (Directory.Exists (dir))
								dir = Path.GetFullPath (dir);
						}
						importContext.Directories.Add (dir);
					}
				}
				
				// Import the referenced libraries
				foreach (XmlElement libElem in node.SelectNodes ("import/widget-library")) {
					string libname = libElem.GetAttribute ("name");
					if (libname.EndsWith (".dll") || libname.EndsWith (".exe")) {
						if (basePath != null && !Path.IsPathRooted (libname)) {
							libname = Path.Combine (basePath, libname);
							if (File.Exists (libname))
								libname = Path.GetFullPath (libname);
						}
					}
					widgetLibraries.Add (libname);
					if (libElem.GetAttribute ("internal") == "true")
						internalLibs.Add (libname);
				}
				
				app.LoadLibraries (importContext, widgetLibraries);
				
				ObjectReader reader = new ObjectReader (this, FileFormat.Native);
				
				if (ownedGlobalActionGroups) {
					foreach (XmlElement groupElem in node.SelectNodes ("action-group")) {
						Wrapper.ActionGroup actionGroup = new Wrapper.ActionGroup ();
						actionGroup.Read (reader, groupElem);
						actionGroups.Add (actionGroup);
					}
				}
				
				XmlElement iconsElem = node.SelectSingleNode ("icon-factory") as XmlElement;
				if (iconsElem != null)
					iconFactory.Read (this, iconsElem);
				
				foreach (XmlElement toplevel in node.SelectNodes ("widget")) {
					Wrapper.Container wrapper = Stetic.ObjectWrapper.ReadObject (reader, toplevel) as Wrapper.Container;
					if (wrapper != null)
						AddWidget ((Gtk.Widget)wrapper.Wrapped);
				}
			} finally {
				loading = false;
			}
		}
		
		public void Save (string fileName)
		{
			this.fileName = fileName;
			XmlDocument doc = Write ();
			
			XmlTextWriter writer = new XmlTextWriter (fileName, System.Text.Encoding.UTF8);
			writer.Formatting = Formatting.Indented;
			doc.Save (writer);
			writer.Close ();
		}
		
		XmlDocument Write ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;

			XmlElement toplevel = doc.CreateElement ("stetic-interface");
			doc.AppendChild (toplevel);
			
			if (widgetLibraries.Count > 0 || (importContext != null && importContext.Directories.Count > 0)) {
				XmlElement importElem = doc.CreateElement ("import");
				toplevel.AppendChild (importElem);
				string basePath = Path.GetDirectoryName (fileName);
				
				if (importContext != null && importContext.Directories.Count > 0) {
					foreach (string dir in importContext.Directories) {
						XmlElement dirElem = doc.CreateElement ("assembly-directory");
						if (basePath != null)
							dirElem.SetAttribute ("path", AbsoluteToRelativePath (basePath, dir));
						else
							dirElem.SetAttribute ("path", dir);
						toplevel.AppendChild (dirElem);
					}
				}
				
				foreach (string wlib in widgetLibraries) {
					string libName = wlib;
					XmlElement libElem = doc.CreateElement ("widget-library");
					if (wlib.EndsWith (".dll") || wlib.EndsWith (".exe")) {
						if (basePath != null)
							libName = AbsoluteToRelativePath (basePath, wlib);
					}

					libElem.SetAttribute ("name", libName);
					if (IsInternalLibrary (wlib))
						libElem.SetAttribute ("internal", "true");
					importElem.AppendChild (libElem);
				}
			}

			ObjectWriter writer = new ObjectWriter (doc, FileFormat.Native);
			if (ownedGlobalActionGroups) {
				foreach (Wrapper.ActionGroup agroup in actionGroups) {
					XmlElement elem = agroup.Write (writer);
					toplevel.AppendChild (elem);
				}
			}
			
			if (iconFactory.Icons.Count > 0)
				toplevel.AppendChild (iconFactory.Write (doc));

			foreach (Widget w in Toplevels) {
				Stetic.Wrapper.Container wrapper = Stetic.Wrapper.Container.Lookup (w);
				if (wrapper == null)
					continue;

				XmlElement elem = wrapper.Write (writer);
				if (elem != null)
					toplevel.AppendChild (elem);
			}
			return doc;
		}
		
		public void ImportGlade (string fileName)
		{
			GladeFiles.Import (this, fileName);
		}
		
		public void ExportGlade (string fileName)
		{
			GladeFiles.Export (this, fileName);
		}
		
		internal void ClipboardCopySelection ()
		{
			Clipboard.Copy (Selection);
		}
		
		public void ClipboardCutSelection ()
		{
			Clipboard.Cut (Selection);
		}
		
		public void ClipboardPaste ()
		{
			Clipboard.Paste (Selection as Placeholder);
		}
		
		public void EditIcons ()
		{
			using (Stetic.Editor.EditIconFactoryDialog dlg = new Stetic.Editor.EditIconFactoryDialog (null, this, this.IconFactory)) {
				dlg.Run ();
			}
		}
		
		public void DeleteSelection ()
		{
			Stetic.Wrapper.Widget wrapper = Stetic.Wrapper.Widget.Lookup (Selection);
			if (wrapper != null)
				wrapper.Delete ();
		}
		
		internal WidgetEditSession CreateWidgetDesignerSession (WidgetDesignerFrontend frontend, string windowName, Stetic.ProjectBackend editingBackend, bool autoCommitChanges)
		{
			foreach (Gtk.Container w in Toplevels) {
				if (w.Name == windowName)
					return new WidgetEditSession (frontend, Stetic.Wrapper.Container.Lookup (w), editingBackend, autoCommitChanges);
			}
			throw new InvalidOperationException ("Component not found: " + windowName);
		}
		
		internal ActionGroupEditSession CreateGlobalActionGroupDesignerSession (ActionGroupDesignerFrontend frontend, string groupName, bool autoCommitChanges)
		{
			return new ActionGroupEditSession (frontend, this, null, groupName, autoCommitChanges);
		}
		
		internal ActionGroupEditSession CreateLocalActionGroupDesignerSession (ActionGroupDesignerFrontend frontend, string windowName, bool autoCommitChanges)
		{
			return new ActionGroupEditSession (frontend, this, windowName, null, autoCommitChanges);
		}
		
		public ArrayList GetTopLevelWrappers ()
		{
			ArrayList list = new ArrayList ();
			foreach (Gtk.Container w in Toplevels)
				list.Add (Component.GetSafeReference (ObjectWrapper.Lookup (w)));
			return list;
		}
		
		public Wrapper.Container GetTopLevelWrapper (string name, bool throwIfNotFound)
		{
			foreach (Gtk.Container w in Toplevels) {
				if (w.Name == name) {
					Wrapper.Container ww = Wrapper.Container.Lookup (w);
					if (ww != null)
						return (Wrapper.Container) Component.GetSafeReference (ww);
					break;
				}
			}
			if (throwIfNotFound)
				throw new InvalidOperationException ("Component not found: " + name);
			return null;
		}
		
		public object AddNewWidget (string type, string name)
		{
			ClassDescriptor cls = Registry.LookupClassByName (type);
			Gtk.Widget w = (Gtk.Widget) cls.NewInstance (this);
			w.Name = name;
			this.AddWidget (w);
			return Component.GetSafeReference (ObjectWrapper.Lookup (w));
		}
		
		public object AddNewWidgetFromTemplate (string template)
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (template);
			Gtk.Widget widget = Stetic.WidgetUtils.ImportWidget (this, doc.DocumentElement);
			AddWidget (widget);
			return Component.GetSafeReference (ObjectWrapper.Lookup (widget));
		}
		
		public void RemoveWidget (string name)
		{
			Wrapper.Widget ww = this.GetTopLevelWrapper (name, false);
			if (ww != null)
				ww.Delete ();
		}
		
		public Stetic.Wrapper.ActionGroup AddNewActionGroup (string name)
		{
			Stetic.Wrapper.ActionGroup group = new Stetic.Wrapper.ActionGroup ();
			group.Name = name;
			ActionGroups.Add (group);
			return group;
		}
		
		public Stetic.Wrapper.ActionGroup AddNewActionGroupFromTemplate (string template)
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (template);
			ObjectReader or = new ObjectReader (this, FileFormat.Native);
			Stetic.Wrapper.ActionGroup group = new Stetic.Wrapper.ActionGroup ();
			group.Read (or, doc.DocumentElement);
			ActionGroups.Add (group);
			return group;
		}
		
		public void RemoveActionGroup (Stetic.Wrapper.ActionGroup group)
		{
			ActionGroups.Remove (group);
		}
		
		public Wrapper.ActionGroup[] GetActionGroups ()
		{
			// Needed since ActionGroupCollection can't be made serializable
			return ActionGroups.ToArray ();
		}
		
		void OnRegistryChanging (object o, EventArgs args)
		{
			if (loading) return;
				
			// Store a copy of the current tree. The tree will
			// be recreated once the registry change is completed.
			
			tempDoc = Write ();
			Selection = null;
		}
		
		void OnRegistryChanged (object o, EventArgs args)
		{
			if (loading) return;
			
			if (tempDoc != null) {
				Read (tempDoc);
				tempDoc = null;
				if (frontend != null)
					frontend.NotifyProjectReloaded ();
				if (ProjectReloaded != null)
					ProjectReloaded (this, EventArgs.Empty);
				NotifyComponentTypesChanged ();
			}
		}
		
		public void Reload ()
		{
			OnRegistryChanging (null, null);
			OnRegistryChanged (null, null);
		}

		public string Id {
			get { return id; }
			set { id = value; }
		}
		
		public bool Modified {
			get { return modified; }
			set {
				if (modified != value) {
					modified = value;
					if (frontend != null)
						frontend.NotifyModifiedChanged ();
					OnModifiedChanged (EventArgs.Empty);
				}
			}
		}
		
		public ImportContext ImportContext {
			get { return importContext; }
		}
		
		public void AddWindow (Gtk.Window window)
		{
			AddWindow (window, false);
		}

		public void AddWindow (Gtk.Window window, bool select)
		{
			AddWidget (window, null, -1);
			if (select)
				Selection = window;
		}

		public void AddWidget (Gtk.Widget widget)
		{
			if (!typeof(Gtk.Container).IsInstanceOfType (widget))
				throw new System.ArgumentException ("widget", "Only containers can be top level widgets");
			AddWidget (widget, null, -1);
		}
		
		void AddWidget (Widget widget, ProjectNode parent)
		{
			AddWidget (widget, parent, -1);
		}

		void AddWidget (Widget widget, ProjectNode parent, int position)
		{
			Stetic.Wrapper.Widget ww = Stetic.Wrapper.Widget.Lookup (widget);
			if (ww == null || ww.Unselectable)
				return;
				
			ww.ObjectChanged += OnObjectChanged;
			ww.NameChanged += OnWidgetNameChanged;
			ww.MemberNameChanged += OnWidgetMemberNameChanged;
			ww.SignalAdded += OnSignalAdded;
			ww.SignalRemoved += OnSignalRemoved;
			ww.SignalChanged += OnSignalChanged;

			ProjectNode node = new ProjectNode (ww);
			node.Id = ++componentIdCounter;
			nodes[widget] = node;
			if (parent == null) {
				topLevels.Add (widget);
				if (position == -1)
					store.AddNode (node);
				else
					store.AddNode (node, position);
			} else {
				if (position == -1)
					parent.AddChild (node);
				else
					parent.AddChild (node, position);
			}
			ww.Destroyed += WidgetDestroyed;

			parent = node;

			if (!loading) {
				if (frontend != null)
					frontend.NotifyWidgetAdded (Component.GetSafeReference (ww), widget.Name, ww.ClassDescriptor.Name, ww.IsTopLevel);
				OnWidgetAdded (new Stetic.Wrapper.WidgetEventArgs (ww));
			}
			
			Stetic.Wrapper.Container container = Stetic.Wrapper.Container.Lookup (widget);
			if (container != null) {
				container.ContentsChanged += ContentsChanged;
				foreach (Gtk.Widget w in container.RealChildren)
					AddWidget (w, parent);
			}
		}

		void UnhashNodeRecursive (ProjectNode node)
		{
			Stetic.Wrapper.Widget ww = Stetic.Wrapper.Widget.Lookup (node.Widget);
			if (ww != null) {
				ww.ObjectChanged -= OnObjectChanged;
				ww.NameChanged -= OnWidgetNameChanged;
				ww.MemberNameChanged -= OnWidgetMemberNameChanged;
				ww.SignalAdded -= OnSignalAdded;
				ww.SignalRemoved -= OnSignalRemoved;
				ww.SignalChanged -= OnSignalChanged;
				Stetic.Wrapper.Container container = ww as Stetic.Wrapper.Container;
				if (container != null)
					container.ContentsChanged -= ContentsChanged;
			}
			ww.Destroyed -= WidgetDestroyed;
			
			if (node.Parent == null)
				topLevels.Remove (node.Widget);

			if (!loading) {
				if (frontend != null)
					frontend.NotifyWidgetRemoved (node.Wrapper, node.Widget.Name, null, node.Parent == null);
				OnWidgetRemoved (new Stetic.Wrapper.WidgetEventArgs (node.Widget));
			}
			
			nodes.Remove (node.Widget);
			
			for (int i = 0; i < node.ChildCount; i++)
				UnhashNodeRecursive (node[i] as ProjectNode);
			node.Widget = null;
			node.Wrapper = null;
		}
		
		void RemoveNode (ProjectNode node)
		{
			UnhashNodeRecursive (node);

			ProjectNode parent = node.Parent as ProjectNode;
			if (parent == null)
				store.RemoveNode (node);
			else
				parent.RemoveChild (node);
		}

		void WidgetDestroyed (object obj, EventArgs args)
		{
			Wrapper.Widget ww = (Wrapper.Widget) obj;
			ProjectNode node = nodes[ww.Wrapped] as ProjectNode;
			if (node != null)
				RemoveNode (node);
		}
		
		void OnObjectChanged (object sender, ObjectWrapperEventArgs args)
		{
			NotifyChanged ();
			if (ObjectChanged != null)
				ObjectChanged (this, args);
		}

		void OnWidgetNameChanged (object sender, Stetic.Wrapper.WidgetNameChangedArgs args)
		{
			NotifyChanged ();
			OnWidgetNameChanged (args);
		}

		protected virtual void OnWidgetNameChanged (Stetic.Wrapper.WidgetNameChangedArgs args)
		{
			if (frontend != null)
				frontend.NotifyWidgetNameChanged (Component.GetSafeReference (args.WidgetWrapper), args.OldName, args.NewName);
			if (WidgetNameChanged != null)
				WidgetNameChanged (this, args);
		}
		
		void OnWidgetMemberNameChanged (object sender, Stetic.Wrapper.WidgetNameChangedArgs args)
		{
			NotifyChanged ();
			OnWidgetMemberNameChanged (args);
		}

		protected virtual void OnWidgetMemberNameChanged (Stetic.Wrapper.WidgetNameChangedArgs args)
		{
			if (WidgetMemberNameChanged != null)
				WidgetMemberNameChanged (this, args);
		}
		
		void OnSignalAdded (object sender, SignalEventArgs args)
		{
			if (frontend != null)
				frontend.NotifySignalAdded (Component.GetSafeReference (args.Wrapper), null, args.Signal);
			OnSignalAdded (args);
		}

		protected virtual void OnSignalAdded (SignalEventArgs args)
		{
			if (SignalAdded != null)
				SignalAdded (this, args);
		}

		void OnSignalRemoved (object sender, SignalEventArgs args)
		{
			if (frontend != null)
				frontend.NotifySignalRemoved (Component.GetSafeReference (args.Wrapper), null, args.Signal);
			OnSignalRemoved (args);
		}

		protected virtual void OnSignalRemoved (SignalEventArgs args)
		{
			if (SignalRemoved != null)
				SignalRemoved (this, args);
		}

		void OnSignalChanged (object sender, SignalChangedEventArgs args)
		{
			OnSignalChanged (args);
		}

		protected virtual void OnSignalChanged (SignalChangedEventArgs args)
		{
			if (frontend != null)
				frontend.NotifySignalChanged (Component.GetSafeReference (args.Wrapper), null, args.OldSignal, args.Signal);
			if (SignalChanged != null)
				SignalChanged (this, args);
		}

		void ContentsChanged (Stetic.Wrapper.Container cwrap)
		{
			Container container = cwrap.Wrapped as Container;
			ProjectNode node = nodes[container] as ProjectNode;
			if (node == null)
				return;

			ArrayList children = new ArrayList ();
			foreach (Gtk.Widget w in cwrap.RealChildren) {
				if (w != null)
					children.Add (w);
			}

			int i = 0;
			while (i < node.ChildCount && i < children.Count) {
				Widget widget = children[i] as Widget;
				ITreeNode child = nodes[widget] as ITreeNode;

				if (child == null)
					AddWidget (widget, node, i);
				else if (child != node[i]) {
					int index = node.IndexOf (child);
					while (index > i) {
						RemoveNode (node[i] as ProjectNode);
						index--;
					}
				}
				i++;
			}

			while (i < node.ChildCount)
				RemoveNode (node[i] as ProjectNode);

			while (i < children.Count)
				AddWidget (children[i++] as Widget, node);

			NotifyChanged ();
		}

		public Gtk.Widget[] Toplevels {
			get {
				return (Gtk.Widget[]) topLevels.ToArray (typeof(Gtk.Widget));
			}
		}

		public void RemoveWidget (Container widget)
		{
			ProjectNode node = nodes[widget] as ProjectNode;
			store.RemoveNode (node);
			UnhashNodeRecursive (node);
		}

		public NodeStore Store {
			get {
				return store;
			}
		}

		public void Clear ()
		{
			nodes.Clear ();
			topLevels.Clear ();
			store = new NodeStore (typeof (ProjectNode));
		}

		// IProject

		public Gtk.Widget Selection {
			get {
				return selection;
			}
			set {
				if (selection == value)
					return;

				Wrapper.ActionGroupCollection newCollection = null;
				
				// FIXME: should there be an IsDestroyed property?
				if (selection != null && selection.Handle != IntPtr.Zero) {
					Stetic.Wrapper.Container parent = Stetic.Wrapper.Container.LookupParent (selection);
					if (parent == null)
						parent = Stetic.Wrapper.Container.Lookup (selection);
					if (parent != null)
						parent.UnSelect (selection);
				}

				selection = value;

				if (selection != null && selection.Handle != IntPtr.Zero) {
					Stetic.Wrapper.Container parent = Stetic.Wrapper.Container.LookupParent (selection);
					if (parent == null)
						parent = Stetic.Wrapper.Container.Lookup (selection);
					if (parent != null)
						parent.Select (selection);
					Wrapper.Widget w = Wrapper.Widget.Lookup (selection);
					if (w != null)
						newCollection = w.LocalActionGroups;
				}

				if (SelectionChanged != null)
					SelectionChanged (this, new Wrapper.WidgetEventArgs (Wrapper.Widget.Lookup (selection)));
				
				if (oldTopActionCollection != newCollection) {
					if (oldTopActionCollection != null)
						oldTopActionCollection.ActionGroupChanged -= OnComponentTypesChanged;
					if (newCollection != null)
						newCollection.ActionGroupChanged += OnComponentTypesChanged;
					oldTopActionCollection = newCollection;
					OnComponentTypesChanged (null, null);
				}
			}
		}

		public void PopupContextMenu (Stetic.Wrapper.Widget wrapper)
		{
			Gtk.Menu m = new ContextMenu (wrapper);
			m.Popup ();
		}

		public void PopupContextMenu (Placeholder ph)
		{
			Gtk.Menu m = new ContextMenu (ph);
			m.Popup ();
		}
		
		internal ProjectNode GetNode (object widget)
		{
			return (ProjectNode) nodes[widget];
		}
		
		internal int GetWidgetId (object widget)
		{
			ProjectNode node = GetNode (widget);
			if (node != null)
				return node.Id;
			else
				return -1;
		}

		internal static string AbsoluteToRelativePath (string baseDirectoryPath, string absPath)
		{
			if (!Path.IsPathRooted (absPath))
				return absPath;
			
			absPath = Path.GetFullPath (absPath);
			baseDirectoryPath = Path.GetFullPath (baseDirectoryPath);
			char[] separators = { Path.DirectorySeparatorChar, Path.VolumeSeparatorChar, Path.AltDirectorySeparatorChar };
			
			string[] bPath = baseDirectoryPath.Split (separators);
			string[] aPath = absPath.Split (separators);
			int indx = 0;
			for(; indx < Math.Min(bPath.Length, aPath.Length); ++indx) {
				if(!bPath[indx].Equals(aPath[indx]))
					break;
			}
			
			if (indx == 0) {
				return absPath;
			}
			
			string erg = "";
			
			if(indx == bPath.Length) {
				erg += "." + Path.DirectorySeparatorChar;
			} else {
				for (int i = indx; i < bPath.Length; ++i) {
					erg += ".." + Path.DirectorySeparatorChar;
				}
			}
			erg += String.Join(Path.DirectorySeparatorChar.ToString(), aPath, indx, aPath.Length-indx);
			
			return erg;
		}
		
		void OnComponentTypesChanged (object s, EventArgs a)
		{
			if (!loading)
				NotifyComponentTypesChanged ();
		}
		
		public void NotifyComponentTypesChanged ()
		{
			if (frontend != null)
				frontend.NotifyComponentTypesChanged ();
			if (ComponentTypesChanged != null)
				ComponentTypesChanged (this, EventArgs.Empty);
		}
		
		void OnGroupAdded (object s, Stetic.Wrapper.ActionGroupEventArgs args)
		{
			args.ActionGroup.SignalAdded += OnSignalAdded;
			args.ActionGroup.SignalRemoved += OnSignalRemoved;
			args.ActionGroup.SignalChanged += OnSignalChanged;
			if (frontend != null)
				frontend.NotifyActionGroupAdded (args.ActionGroup.Name);
			OnComponentTypesChanged (null, null);
		}
		
		void OnGroupRemoved (object s, Stetic.Wrapper.ActionGroupEventArgs args)
		{
			args.ActionGroup.SignalAdded -= OnSignalAdded;
			args.ActionGroup.SignalRemoved -= OnSignalRemoved;
			args.ActionGroup.SignalChanged -= OnSignalChanged;
			if (frontend != null)
				frontend.NotifyActionGroupRemoved (args.ActionGroup.Name);
			OnComponentTypesChanged (null, null);
		}
		
		void NotifyChanged ()
		{
			Modified = true;
			if (frontend != null)
				frontend.NotifyChanged ();
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}
		
		protected virtual void OnModifiedChanged (EventArgs args)
		{
			if (ModifiedChanged != null)
				ModifiedChanged (this, args);
		}
		
		protected virtual void OnWidgetRemoved (Stetic.Wrapper.WidgetEventArgs args)
		{
			NotifyChanged ();
			if (WidgetRemoved != null)
				WidgetRemoved (this, args);
		}
		
		protected virtual void OnWidgetAdded (Stetic.Wrapper.WidgetEventArgs args)
		{
			NotifyChanged ();
			if (WidgetAdded != null)
				WidgetAdded (this, args);
		}
	}

	[TreeNode (ColumnCount=2)]
	public class ProjectNode : TreeNode {
		Widget widget;
		ClassDescriptor klass;
		int id;
		Wrapper.Widget wrapper;

		public ProjectNode (Stetic.Wrapper.Widget wrapper)
		{
			this.widget = (Widget) wrapper.Wrapped;
			this.wrapper = wrapper;
			klass = wrapper.ClassDescriptor;
		}
		
		public int Id {
			get { return id; }
			set { id = value; }
		}

		public Widget Widget {
			get {
				return widget;
			}
			set {
				widget = value;
			}
		}

		public Wrapper.Widget Wrapper {
			get {
				return wrapper;
			}
			set {
				wrapper = value;
			}
		}

		[TreeNodeValue (Column=0)]
		public Gdk.Pixbuf Icon {
			get {
				return klass.Icon;
			}
		}

		[TreeNodeValue (Column=1)]
		public string Name {
			get {
				if (widget == null)
					return "";
				else
					return widget.Name;
			}
		}
		
		public ProjectNode RootNode {
			get {
				ProjectNode p = this;
				while (p.Parent != null)
					p = (ProjectNode) p.Parent;
				return p;
			}
		}
		
		public override string ToString ()
		{
			return "[ProjectNode " + GetHashCode().ToString() + " " + widget.GetType().FullName + " '" + Name + "']";
		}
	}
	
	class ImportContext
	{
		public StringCollection Directories = new StringCollection ();
	}
}
