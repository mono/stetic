
using System;
using System.Xml;
using System.Collections;

namespace Stetic.Wrapper
{
	
	public class ActionTree: ActionTreeNode
	{
		public ActionTree()
		{
		}
	}
	
	public class ActionTreeNode
	{
		Gtk.UIManagerItemType type;
		string name;
		Action action;
		ActionTreeNodeCollection children;
		ActionTreeNode parentNode;
		
		public ActionTreeNode ()
		{
		}
		
		public ActionTreeNode Clone ()
		{
			return new ActionTreeNode (type, name, action);
		}
		
		public ActionTreeNode (Gtk.UIManagerItemType type, string name, Action action)
		{
			this.type = type;
			this.name = name;
			this.action = action;
		}

		public XmlElement Write (XmlDocument doc, FileFormat format)
		{
			XmlElement elem = doc.CreateElement ("node");
			if (name != null && name.Length > 0)
				elem.SetAttribute ("name", name);
			elem.SetAttribute ("type", type.ToString ());
			elem.SetAttribute ("action", action.GtkAction.Name);
			
			if (children != null) {
				foreach (ActionTreeNode child in children)
					elem.AppendChild (child.Write (doc, format));
			}
			return elem;
		}
		
		public void Read (Wrapper.Widget baseWidget, XmlElement elem)
		{
			name = elem.GetAttribute ("name");
			if (elem.HasAttribute ("type"))
				type = (Gtk.UIManagerItemType) Enum.Parse (typeof(Gtk.UIManagerItemType), elem.GetAttribute ("type"));
			
			string aname = elem.GetAttribute ("action");
			if (aname.Length > 0) {
				action = baseWidget.LocalActionGroup.GetAction (aname);
				if (action == null) {
					foreach (ActionGroup group in baseWidget.Project.ActionGroups) {
						action = group.GetAction (aname);
						if (action == null)
							break;
					}
				}
			}
			foreach (XmlElement child in elem.SelectNodes ("node")) {
				ActionTreeNode node = new ActionTreeNode ();
				node.Read (baseWidget, child);
				Children.Add (node);
			}
		}
		
		public Gtk.UIManagerItemType Type {
			get { return type; }
			set { type = value; }
		}
		
		public string Name {
			get { return name; }
			set { name = value; }
		}
		
		public Action Action {
			get { return action; }
			set { action = value; }
		}
		
		public ActionTreeNode ParentNode {
			get { return parentNode; }
		}
		
		public ActionTreeNodeCollection Children {
			get {
				if (children == null)
					children = new ActionTreeNodeCollection (this);
				return children;
			}
		}
		
		internal void NotifyChildAdded (ActionTreeNode node)
		{
			node.parentNode = this;
			if (ChildNodeAdded != null)
				ChildNodeAdded (this, new ActionTreeNodeArgs (node));
		}
		
		internal void NotifyChildRemoved (ActionTreeNode node)
		{
			node.parentNode = null;
			if (ChildNodeRemoved != null)
				ChildNodeRemoved (this, new ActionTreeNodeArgs (node));
		}
		
		public event ActionTreeNodeHanlder ChildNodeAdded;
		public event ActionTreeNodeHanlder ChildNodeRemoved;
	}
	
	public class ActionTreeNodeCollection: CollectionBase
	{
		ActionTreeNode parent;
		
		public ActionTreeNodeCollection (ActionTreeNode parent)
		{
			this.parent = parent;
		}
		
		public void Add (ActionTreeNode node)
		{
			List.Add (node);
		}
		
		public void Insert (int index, ActionTreeNode node)
		{
			List.Insert (index, node);
		}
		
		public int IndexOf (ActionTreeNode node)
		{
			return List.IndexOf (node);
		}
		
		public void Remove (ActionTreeNode node)
		{
			List.Remove (node);
		}
		
		public ActionTreeNode this [int n] {
			get { return (ActionTreeNode) List [n]; }
			set { List [n] = value; }
		}

		protected override void OnInsertComplete (int index, object val)
		{
			parent.NotifyChildAdded ((ActionTreeNode) val);
		}
		
		protected override void OnRemoveComplete (int index, object val)
		{
			parent.NotifyChildRemoved ((ActionTreeNode)val);
		}
		
		protected override void OnSetComplete (int index, object oldv, object newv)
		{
			parent.NotifyChildRemoved ((ActionTreeNode) oldv);
			parent.NotifyChildAdded ((ActionTreeNode) newv);
		}
	}
	
	public delegate void ActionTreeNodeHanlder (object ob, ActionTreeNodeArgs args);
	
	public class ActionTreeNodeArgs: EventArgs
	{
		readonly ActionTreeNode node;
		
		public ActionTreeNodeArgs (ActionTreeNode node)
		{
			this.node = node;
		}
		
		public ActionTreeNode Node {
			get { return node; }
		}
	}
	
}
