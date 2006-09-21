using Gtk;
using System;
using System.Xml;
using System.Collections;
using System.Collections.Specialized;
using System.CodeDom;
using Mono.Unix;

namespace Stetic {

	internal class ProjectBackend : MarshalByRefObject, IProject, IDisposable 
	{
		Hashtable nodes;
		NodeStore store;
		bool modified;
		internal bool Syncing;
		Gtk.Widget selection;
		string id;
		string fileName;
		XmlDocument tempDoc;
		bool loading;
		IResourceProvider resourceProvider;
		Stetic.Wrapper.ActionGroupCollection actionGroups;
		Stetic.ProjectIconFactory iconFactory;
		Project frontend;
		int componentIdCounter;
		WidgetLibrarySet widgetLibraries;
		
		public event Wrapper.WidgetNameChangedHandler WidgetNameChanged;
		public event Wrapper.WidgetNameChangedHandler WidgetMemberNameChanged;
		public event Wrapper.WidgetEventHandler WidgetAdded;
		public event Wrapper.WidgetEventHandler WidgetRemoved;
		
		public event SignalEventHandler SignalAdded;
		public event SignalEventHandler SignalRemoved;
		public event SignalChangedEventHandler SignalChanged;
		
		public event Wrapper.WidgetEventHandler SelectionChanged;
		public event EventHandler ModifiedChanged;
		
		// Fired when the project has been reloaded, due for example to
		// a change in the registry
		public event EventHandler ProjectReloaded;

		public ProjectBackend ()
		{
			nodes = new Hashtable ();
			store = new NodeStore (typeof (ProjectNode));
			
			ActionGroups = new Stetic.Wrapper.ActionGroupCollection ();

			Registry.RegistryChanging += OnRegistryChanging;
			Registry.RegistryChanged += OnRegistryChanged;
			
			iconFactory = new ProjectIconFactory ();
		}
		
		public void Dispose ()
		{
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
		
		public WidgetLibrarySet WidgetLibraries {
			get { return widgetLibraries; }
			set { widgetLibraries = value; }
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
				}
				actionGroups = value;
				if (actionGroups != null) {
					actionGroups.ActionGroupAdded += OnGroupAdded;
					actionGroups.ActionGroupRemoved += OnGroupRemoved;
				}
			}
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
			
			if (actionGroups != null) {
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

			iconFactory = new ProjectIconFactory ();
		}
		
		public void Load (string fileName)
		{
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			doc.Load (fileName);
			Read (doc);
			
			this.fileName = fileName;
			Id = System.IO.Path.GetFileName (fileName);
		}
		
		public void Read (XmlDocument doc)
		{
			loading = true;
			
			try {
				Close ();
				
				XmlNode node = doc.SelectSingleNode ("/stetic-interface");
				if (node == null)
					throw new ApplicationException (Catalog.GetString ("Not a Stetic file according to node name."));
				
				foreach (XmlElement groupElem in node.SelectNodes ("action-group")) {
					Wrapper.ActionGroup actionGroup = new Wrapper.ActionGroup ();
					actionGroup.Read (this, groupElem);
					actionGroups.Add (actionGroup);
				}
				
				XmlElement iconsElem = node.SelectSingleNode ("icon-factory") as XmlElement;
				if (iconsElem != null)
					iconFactory.Read (this, iconsElem);
				
				foreach (XmlElement toplevel in node.SelectNodes ("widget")) {
					Wrapper.Container wrapper = Stetic.ObjectWrapper.Read (this, toplevel, FileFormat.Native) as Wrapper.Container;
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
		
		public XmlDocument Write ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;

			XmlElement toplevel = doc.CreateElement ("stetic-interface");
			doc.AppendChild (toplevel);

			foreach (Wrapper.ActionGroup agroup in actionGroups) {
				XmlElement elem = agroup.Write (doc, FileFormat.Native);
				toplevel.AppendChild (elem);
			}
			
			if (iconFactory.Icons.Count > 0)
				toplevel.AppendChild (iconFactory.Write (doc));

			foreach (Widget w in Toplevels) {
				Stetic.Wrapper.Container wrapper = Stetic.Wrapper.Container.Lookup (w);
				if (wrapper == null)
					continue;

				XmlElement elem = wrapper.Write (doc, FileFormat.Native);
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
			Stetic.Wrapper.ActionGroup group = new Stetic.Wrapper.ActionGroup ();
			group.Read (this, doc.DocumentElement);
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
			// Store a copy of the current tree. The tree will
			// be recreated once the registry change is completed.
			
			tempDoc = Write ();
			Selection = null;
		}
		
		void OnRegistryChanged (object o, EventArgs args)
		{
			if (tempDoc != null) {
				Read (tempDoc);
				tempDoc = null;
				if (ProjectReloaded != null)
					ProjectReloaded (this, EventArgs.Empty);
			}
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
			if (!Syncing)
				Modified = true;
		}

		void OnWidgetNameChanged (object sender, Stetic.Wrapper.WidgetNameChangedArgs args)
		{
			if (!Syncing) {
				Modified = true;
				OnWidgetNameChanged (args);
			}
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
			if (!Syncing) {
				Modified = true;
				OnWidgetMemberNameChanged (args);
			}
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
				frontend.NotifySignalRemoved (Component.GetSafeReference (args.Wrapper), ((Gtk.Widget)args.Wrapper.Wrapped).Name, args.Signal);
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
				frontend.NotifySignalChanged (Component.GetSafeReference (args.Wrapper), ((Gtk.Widget)args.Wrapper.Wrapped).Name, args.OldSignal, args.Signal);
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

			Modified = true;
		}

		public Gtk.Widget[] Toplevels {
			get {
				ArrayList list = new ArrayList ();
				foreach (Widget w in nodes.Keys) {
					Wrapper.Widget wrapper = Wrapper.Widget.Lookup (w);
					if (wrapper != null && wrapper.IsTopLevel)
						list.Add (w);
				}
				return (Gtk.Widget[]) list.ToArray (typeof(Gtk.Widget));
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
				}

				if (frontend != null) {
					Gtk.Widget w = selection as Gtk.Widget;
					if (w != null) {
						ProjectNode node = GetNode (w);
						if (node != null) {
							Stetic.Wrapper.Widget ww = Stetic.Wrapper.Widget.Lookup (w);
							frontend.NotifySelectionChanged (Component.GetSafeReference (ww), w.Name, ww.ClassDescriptor.Name);
						} else {
							// FIXME
							frontend.NotifySelectionChanged ("__placeholder", null, null);
						}
					} else if (selection == null) {
						frontend.NotifySelectionChanged (null, null, null);
					}
				}
				if (SelectionChanged != null)
					SelectionChanged (this, new Wrapper.WidgetEventArgs (Wrapper.Widget.Lookup (selection)));
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
		
		void OnGroupAdded (object s, Stetic.Wrapper.ActionGroupEventArgs args)
		{
			args.ActionGroup.SignalAdded += OnSignalAdded;
			args.ActionGroup.SignalRemoved += OnSignalRemoved;
			args.ActionGroup.SignalChanged += OnSignalChanged;
			if (frontend != null)
				frontend.NotifyActionGroupAdded (args.ActionGroup.Name);
		}
		
		void OnGroupRemoved (object s, Stetic.Wrapper.ActionGroupEventArgs args)
		{
			args.ActionGroup.SignalAdded -= OnSignalAdded;
			args.ActionGroup.SignalRemoved -= OnSignalRemoved;
			args.ActionGroup.SignalChanged -= OnSignalChanged;
			if (frontend != null)
				frontend.NotifyActionGroupRemoved (args.ActionGroup.Name);
		}
		
		protected virtual void OnModifiedChanged (EventArgs args)
		{
			if (ModifiedChanged != null)
				ModifiedChanged (this, args);
		}
		
		protected virtual void OnWidgetRemoved (Stetic.Wrapper.WidgetEventArgs args)
		{
			Modified = true;
			if (WidgetRemoved != null)
				WidgetRemoved (this, args);
		}
		
		protected virtual void OnWidgetAdded (Stetic.Wrapper.WidgetEventArgs args)
		{
			Modified = true;
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
}
