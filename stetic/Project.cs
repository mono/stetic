using Gtk;
using System;
using System.Collections;

namespace Stetic {

	// FIXME 71749
	public class Project {
		Hashtable nodes;
		NodeStore store;

		public Project ()
		{
			nodes = new Hashtable ();
			store = new NodeStore (typeof (ProjectNode));
		}

		public void AddWindow (Widget window)
		{
			AddWidget (window, null);
		}

		void AddWidget (Widget widget, ProjectNode parent)
		{
			if (Stetic.Wrapper.Widget.Lookup (widget) != null) {
				ProjectNode node = new ProjectNode (widget);
				nodes[widget] = node;
				if (parent == null)
					store.AddNode (node);
				else
					parent.AddChild (node);

				parent = node;

				Stetic.Wrapper.Container container = Stetic.Wrapper.Container.Lookup (widget);
				if (container != null)
					container.ContentsChanged += ContentsChanged;
			}

			if (widget is Container) {
				Container container = (Container)widget;
				foreach (Widget child in new SiteContentEnumerator (container))
					AddWidget (child, parent);
			}
		}

		void ContentsChanged (Stetic.Wrapper.Container cwrap)
		{
			Container container = cwrap.Wrapped as Container;
			ProjectNode node = nodes[container] as ProjectNode, child;

			// Since TreeNode doesn't have an InsertChild method, the
			// easiest way to do this is to copy all of the child nodes
			// out, and then add them (and any new ones) back in in
			// the right order.

			Hashtable childNodes = new Hashtable ();
			while (node.ChildCount != 0) {
				child = node[0] as ProjectNode;
				childNodes[child.Widget] = child;
				node.RemoveChild (child);
			}

			foreach (Widget w in new SiteContentEnumerator (container)) {
				child = childNodes[w] as ProjectNode;
				if (child != null) {
					childNodes.Remove (w);
					node.AddChild (child);
				} else
					AddWidget (w, node);
			}

			foreach (ProjectNode dead in childNodes.Values)
				nodes.Remove (dead.Widget);
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
	}


}
