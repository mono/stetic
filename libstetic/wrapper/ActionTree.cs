
using System;
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
		
		public ActionTreeNodeCollection Children {
			get {
				if (children == null)
					children = new ActionTreeNodeCollection ();
				return children;
			}
		}
	}
	
	public class ActionTreeNodeCollection: CollectionBase
	{
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
		
		public ActionTreeNode this [int n] {
			get { return (ActionTreeNode) List [n]; }
		}
	}
}
