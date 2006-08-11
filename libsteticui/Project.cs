using Gtk;
using System;
using System.Xml;
using System.Collections;
using System.CodeDom;

namespace Stetic {

	public class Project : IProject, IDisposable {
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
		
		public event Wrapper.WidgetNameChangedHandler WidgetNameChanged;
		public event Wrapper.WidgetNameChangedHandler WidgetMemberNameChanged;
		public event Wrapper.WidgetEventHandler WidgetAdded;
		public event Wrapper.WidgetEventHandler WidgetRemoved;
		
		public event SignalEventHandler SignalAdded;
		public event SignalEventHandler SignalRemoved;
		public event SignalChangedEventHandler SignalChanged;
		
		public event Wrapper.WidgetEventHandler SelectionChanged;
		public event EventHandler ModifiedChanged;
		
		public event IProjectDelegate GladeImportComplete;
		
		// Fired when the project has been reloaded, due for example to
		// a change in the registry
		public event EventHandler ProjectReloaded;

		public Project ()
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
			ActionGroups = null;
			Registry.RegistryChanging -= OnRegistryChanging;
			Registry.RegistryChanged -= OnRegistryChanged;
			foreach (Gtk.Widget w in Toplevels)
				w.Destroy ();
		}
		
		public string FileName {
			get { return fileName; }
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
		
		public void Load (string fileName)
		{
			this.fileName = fileName;
			
			Id = System.IO.Path.GetFileName (fileName);
			
			XmlDocument doc = new XmlDocument ();
			doc.PreserveWhitespace = true;
			doc.Load (fileName);
			Read (doc);
		}
		
		public void Read (XmlDocument doc)
		{
			loading = true;
			
			try {
				// Clean the existing tree
				foreach (Gtk.Widget w in Toplevels)
					w.Destroy ();

				selection = null;
				store.Clear ();
				nodes.Clear ();
				
				XmlNode node = doc.SelectSingleNode ("/stetic-interface");
				if (node == null)
					throw new ApplicationException ("Not a Stetic file according to node name");
				
				actionGroups.Clear ();
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
			if (ww == null)
				return;
				
			ww.ObjectChanged += OnObjectChanged;
			ww.NameChanged += OnWidgetNameChanged;
			ww.MemberNameChanged += OnWidgetMemberNameChanged;
			ww.SignalAdded += OnSignalAdded;
			ww.SignalRemoved += OnSignalRemoved;
			ww.SignalChanged += OnSignalChanged;

			ProjectNode node = new ProjectNode (ww);
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
			widget.Destroyed += WidgetDestroyed;

			parent = node;

			if (!loading)
				OnWidgetAdded (new Stetic.Wrapper.WidgetEventArgs (ww));
			
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
			ww.ObjectChanged -= OnObjectChanged;
			ww.NameChanged -= OnWidgetNameChanged;
			ww.MemberNameChanged -= OnWidgetMemberNameChanged;
			ww.SignalAdded -= OnSignalAdded;
			ww.SignalRemoved -= OnSignalRemoved;
			ww.SignalChanged -= OnSignalChanged;
			((Gtk.Widget)ww.Wrapped).Destroyed -= WidgetDestroyed;
			
			if (!loading)
				OnWidgetRemoved (new Stetic.Wrapper.WidgetEventArgs (ww));
			
			nodes.Remove (node.Widget);
			for (int i = 0; i < node.ChildCount; i++)
				UnhashNodeRecursive (node[i] as ProjectNode);
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
			ProjectNode node = nodes[obj] as ProjectNode;
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
			OnSignalAdded (args);
		}

		protected virtual void OnSignalAdded (SignalEventArgs args)
		{
			if (SignalAdded != null)
				SignalAdded (this, args);
		}

		void OnSignalRemoved (object sender, SignalEventArgs args)
		{
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

		public Gtk.Widget LookupWidgetById (string id)
		{
			foreach (Gtk.Widget w in nodes.Keys) {
				if (w.Name == id)
					return w;
			}
			return null;
		}
		
		public void BeginGladeImport ()
		{
		}

		public void EndGladeImport ()
		{
			if (GladeImportComplete != null)
				GladeImportComplete ();
		}
		
		internal ProjectNode GetNode (object widget)
		{
			return (ProjectNode) nodes[widget];
		}
		
		void OnGroupAdded (object s, Stetic.Wrapper.ActionGroupEventArgs args)
		{
			args.ActionGroup.SignalAdded += OnSignalAdded;
			args.ActionGroup.SignalRemoved += OnSignalRemoved;
			args.ActionGroup.SignalChanged += OnSignalChanged;
		}
		
		void OnGroupRemoved (object s, Stetic.Wrapper.ActionGroupEventArgs args)
		{
			args.ActionGroup.SignalAdded -= OnSignalAdded;
			args.ActionGroup.SignalRemoved -= OnSignalRemoved;
			args.ActionGroup.SignalChanged -= OnSignalChanged;
		}
		
		protected virtual void OnModifiedChanged (EventArgs args)
		{
			if (ModifiedChanged != null)
				ModifiedChanged (this, args);
		}
		
		protected virtual void OnWidgetRemoved (Stetic.Wrapper.WidgetEventArgs args)
		{
			if (WidgetRemoved != null)
				WidgetRemoved (this, args);
		}
		
		protected virtual void OnWidgetAdded (Stetic.Wrapper.WidgetEventArgs args)
		{
			if (WidgetAdded != null)
				WidgetAdded (this, args);
		}
	}

	[TreeNode (ColumnCount=2)]
	public class ProjectNode : TreeNode {
		Widget widget;
		ClassDescriptor klass;

		public ProjectNode (Stetic.Wrapper.Widget wrapper)
		{
			this.widget = (Widget) wrapper.Wrapped;
			klass = wrapper.ClassDescriptor;
		}

		public Widget Widget {
			get {
				return widget;
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
				return widget.Name;
			}
		}

		public override string ToString ()
		{
			return "[ProjectNode " + GetHashCode().ToString() + " " + widget.GetType().FullName + " '" + Name + "']";
		}
	}
}
