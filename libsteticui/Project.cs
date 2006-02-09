using Gtk;
using System;
using System.Collections;

namespace Stetic {

	public class Project : IProject {
		Hashtable nodes;
		NodeStore store;
		bool modified;
		internal bool Syncing;
		Gtk.Widget selection;
		
		public event Wrapper.WidgetNameChangedHandler WidgetNameChanged;
		public event Wrapper.WidgetEventHandler WidgetAdded;
		public event Wrapper.WidgetEventHandler WidgetRemoved;
		
		public event Wrapper.SignalEventHandler SignalAdded;
		public event Wrapper.SignalEventHandler SignalRemoved;
		public event Wrapper.SignalChangedEventHandler SignalChanged;
		
		public event SelectedHandler Selected;

		public event EventHandler ModifiedChanged;

		public Project ()
		{
			nodes = new Hashtable ();
			store = new NodeStore (typeof (ProjectNode));
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

			ww.WidgetChanged += OnWidgetChanged;
			ww.NameChanged += OnWidgetNameChanged;
			ww.SignalAdded += OnSignalAdded;
			ww.SignalRemoved += OnSignalRemoved;
			ww.SignalChanged += OnSignalChanged;

			ProjectNode node = new ProjectNode (widget);
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
			ww.WidgetChanged -= OnWidgetChanged;
			ww.NameChanged -= OnWidgetNameChanged;
			ww.SignalAdded -= OnSignalAdded;
			ww.SignalRemoved -= OnSignalRemoved;
			ww.SignalChanged -= OnSignalChanged;
			
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
		
		void OnWidgetChanged (object sender, Wrapper.WidgetEventArgs args)
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
		
		void OnSignalAdded (object sender, Wrapper.SignalEventArgs args)
		{
			OnSignalAdded (args);
		}

		protected virtual void OnSignalAdded (Wrapper.SignalEventArgs args)
		{
			if (SignalAdded != null)
				SignalAdded (this, args);
		}

		void OnSignalRemoved (object sender, Wrapper.SignalEventArgs args)
		{
			OnSignalRemoved (args);
		}

		protected virtual void OnSignalRemoved (Wrapper.SignalEventArgs args)
		{
			if (SignalRemoved != null)
				SignalRemoved (this, args);
		}

		void OnSignalChanged (object sender, Wrapper.SignalChangedEventArgs args)
		{
			OnSignalChanged (args);
		}

		protected virtual void OnSignalChanged (Wrapper.SignalChangedEventArgs args)
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

		public IEnumerable Toplevels {
			get {
				ArrayList list = new ArrayList ();
				foreach (Widget w in nodes.Keys) {
					if (w is Gtk.Window)
						list.Add (w);
				}
				return list;
			}
		}

		public void DeleteWindow (Widget window)
		{
			store.RemoveNode (nodes[window] as ProjectNode);
			nodes.Remove (window);
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

		public delegate void SelectedHandler (Gtk.Widget selection, ProjectNode node);

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

				if (Selected != null)
					Selected (selection, selection == null ? null : nodes[selection] as ProjectNode);
			}
		}

		Stetic.Tooltips tooltips;
		public Stetic.Tooltips Tooltips {
			get {
				if (tooltips == null)
					tooltips = new Stetic.Tooltips ();
				return tooltips;
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

		public event IProjectDelegate GladeImportComplete;

		public void BeginGladeImport ()
		{
			;
		}

		public void EndGladeImport ()
		{
			if (GladeImportComplete != null)
				GladeImportComplete ();
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

		public ProjectNode (Gtk.Widget widget)
		{
			this.widget = widget;
			klass = Registry.LookupClass (widget.GetType ());
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
