using Gtk;
using System;
using System.Collections;

namespace Stetic {

	public class Project : IStetic {
		Hashtable nodes;
		NodeStore store;

		public Project ()
		{
			nodes = new Hashtable ();
			store = new NodeStore (typeof (ProjectNode));
		}

		public void AddWindow (WindowSite site)
		{
			AddWindow (site, false);
		}

		public void AddWindow (WindowSite site, bool select)
		{
			site.FocusChanged += WindowFocusChanged;
			AddWidget (site.Contents, null, -1);

			if (select)
				WindowFocusChanged (site, site);
		}

		void AddWidget (Widget widget, ProjectNode parent)
		{
			AddWidget (widget, parent, -1);
		}

		void AddWidget (Widget widget, ProjectNode parent, int position)
		{
			if (Stetic.Wrapper.Widget.Lookup (widget) == null)
				return;

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

			Stetic.Wrapper.Container container = Stetic.Wrapper.Container.Lookup (widget);
			if (container != null) {
				container.ContentsChanged += ContentsChanged;
				foreach (WidgetSite site in container.Sites) {
					if (site.Occupied)
						AddWidget (site.Contents, parent);
				}
			}
		}

		void UnhashNodeRecursive (ProjectNode node)
		{
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

		void ContentsChanged (Stetic.Wrapper.Container cwrap)
		{
			Container container = cwrap.Wrapped as Container;
			ProjectNode node = nodes[container] as ProjectNode;
			if (node == null)
				return;

			ArrayList children = new ArrayList ();
			foreach (WidgetSite site in cwrap.Sites) {
				if (site.Occupied)
					children.Add (site.Contents);
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

		public delegate void SelectedHandler (IWidgetSite site, ProjectNode node);
		public event SelectedHandler Selected;

		void WindowFocusChanged (WindowSite site, IWidgetSite focus)
		{
			if (Selected == null)
				return;

			if (focus == null)
				Selected (null, null);
			else
				Selected (focus, nodes[focus.Contents] as ProjectNode);
		}

		// IStetic

		public WidgetSite CreateWidgetSite ()
		{
			WidgetSite site = new WidgetSite (this);
			site.PopupContextMenu += delegate (object obj, EventArgs args) {
				Menu m = new ContextMenu ((WidgetSite)site);
				m.Popup ();
			};
			return site;
		}

		public Gtk.Widget LookupWidgetById (string id)
		{
			foreach (Gtk.Widget w in nodes.Keys) {
				if (w.Name == id)
					return w;
			}
			return null;
		}

		public event ISteticDelegate GladeImportComplete;

		public void BeginGladeImport ()
		{
			;
		}

		public void EndGladeImport ()
		{
			if (GladeImportComplete != null)
				GladeImportComplete ();
		}
	}

	[TreeNode (ColumnCount=2)]
	public class ProjectNode : TreeNode {
		Widget widget;
		Gdk.Pixbuf icon;

		public ProjectNode (Widget widget)
		{
			this.widget = widget;
			Stetic.Wrapper.Object wrapper = Stetic.Wrapper.Object.Lookup (widget);
			icon = Stetic.Palette.IconForType (wrapper.GetType ());
		}

		public Widget Widget {
			get {
				return widget;
			}
		}

		[TreeNodeValue (Column=0)]
		public Gdk.Pixbuf Icon {
			get {
				return icon;
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
