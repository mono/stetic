
using System;
using System.Xml;
using System.Collections;
using Stetic.Editor;

namespace Stetic.Wrapper
{
	public class MenuBar: Container
	{
		ActionTree actionTree;
		
		public MenuBar()
		{
		}
		
		public static new Gtk.MenuBar CreateInstance ()
		{
			return new ActionMenuBar ();
		}
		
		ActionMenuBar menu {
			get { return (ActionMenuBar) Wrapped; }
		}
		
		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			
			actionTree = new ActionTree ();
			
			Action ac1 = (Action) ObjectWrapper.Create (Project, new Gtk.Action ("File", "File", null, Gtk.Stock.Open));
			Action ac2 = (Action) ObjectWrapper.Create (Project, new Gtk.Action ("Open", null, null, Gtk.Stock.Open));
			Action ac3 = (Action) ObjectWrapper.Create (Project, new Gtk.Action ("Close", "Close", null, null));
			ac3.Type = Action.ActionType.Toggle;
			Action ac4 = (Action) ObjectWrapper.Create (Project, new Gtk.Action ("Documents", "Documents", null, null));
			Action ac5 = (Action) ObjectWrapper.Create (Project, new Gtk.Action ("Doc1", "Doc1", null, null));
			ac5.Type = Action.ActionType.Radio;
			Action ac6 = (Action) ObjectWrapper.Create (Project, new Gtk.Action ("MoreDocs", "MoreDocs", null, null));
			Action ac7 = (Action) ObjectWrapper.Create (Project, new Gtk.Action ("Doc2", "Doc2", null, null));
			
			ActionTreeNode node = new ActionTreeNode (Gtk.UIManagerItemType.Menu, null, ac1);
			actionTree.Children.Add (node);
			ActionTreeNode cnode = new ActionTreeNode (Gtk.UIManagerItemType.Menuitem, null, ac2);
			node.Children.Add (cnode);
			cnode = new ActionTreeNode (Gtk.UIManagerItemType.Menuitem, null, ac3);
			node.Children.Add (cnode);
			cnode = new ActionTreeNode (Gtk.UIManagerItemType.Menu, null, ac4);
			node.Children.Add (cnode);
			
			node = cnode;
			cnode = new ActionTreeNode (Gtk.UIManagerItemType.Menuitem, null, ac5);
			node.Children.Add (cnode);
			ActionTreeNode cnode2 = new ActionTreeNode (Gtk.UIManagerItemType.Menu, null, ac6);
			node.Children.Add (cnode2);
			cnode = new ActionTreeNode (Gtk.UIManagerItemType.Menuitem, null, ac7);
			node.Children.Add (cnode);
			
			cnode = new ActionTreeNode (Gtk.UIManagerItemType.Menuitem, null, ac5);
			cnode2.Children.Add (cnode);
			cnode = new ActionTreeNode (Gtk.UIManagerItemType.Menuitem, null, ac7);
			cnode2.Children.Add (cnode);
			
			node = new ActionTreeNode (Gtk.UIManagerItemType.Menu, null, ac4);
			actionTree.Children.Add (node);
			cnode = new ActionTreeNode (Gtk.UIManagerItemType.Menuitem, null, ac5);
			node.Children.Add (cnode);
			menu.FillMenu (actionTree);
		}
		
		internal protected override void OnSelected ()
		{
			menu.ShowInsertPlaceholder = true;
		}
		
		internal protected override void OnUnselected ()
		{
			base.OnUnselected ();
			menu.OpenSubmenu = null;
			menu.ShowInsertPlaceholder = false;
		}
		
		public override XmlElement Write (XmlDocument doc, FileFormat format)
		{
			XmlElement elem = base.Write (doc, format);
			elem.AppendChild (actionTree.Write (doc, format));
			return elem;
		}
		
		public override void Read (XmlElement elem, FileFormat format)
		{
			base.Read (elem, format);
			actionTree = new ActionTree ();
			actionTree.Read (this, elem);
			menu.FillMenu (actionTree);
		}
	}
	
	class CustomMenuBarItem: Gtk.MenuItem
	{
		public ActionMenuItem ActionMenuItem;
		public ActionTreeNode Node;
	}
		
	public class ActionPaletteItem: Gtk.HBox
	{
		ActionTreeNode node;
		
		public ActionPaletteItem (Gtk.UIManagerItemType type, string name, Action action) 
		: this (new ActionTreeNode (type, name, action))
		{
		}
		
		public ActionPaletteItem (ActionTreeNode node)
		{
			this.node = node;
			Spacing = 3;
			if (node.Type == Gtk.UIManagerItemType.Menu) {
				PackStart (new Gtk.Label ("Menu"), true, true, 0);
			} else if (node.Action != null && node.Action.GtkAction != null) {
				if (node.Action.GtkAction.StockId != null)
					PackStart (node.Action.GtkAction.CreateIcon (Gtk.IconSize.Menu), true, true, 0);
				PackStart (new Gtk.Label (node.Action.GtkAction.Label), true, true, 0);
			} else {
				PackStart (new Gtk.Label ("Empty Action"), true, true, 0);
			}
			ShowAll ();
		}
		
		public ActionTreeNode Node {
			get { return node; }
		}
	}
}
