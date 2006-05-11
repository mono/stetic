
using System;
using System.Xml;
using System.Collections;
using Stetic.Wrapper;

namespace Stetic.Editor
{
	class ActionMenuBar: Gtk.MenuBar, IMenuItemContainer
	{
		ActionMenu openSubmenu;
		ActionTree actionTree;
		int dropPosition = -1;
		int dropIndex;
		ArrayList menuItems = new ArrayList ();
		bool showPlaceholder;
		
		public ActionMenuBar ()
		{
			DND.DestSet (this, true);
		}
		
		public void FillMenu (ActionTree actionTree)
		{
			if (this.actionTree != null) {
				this.actionTree.ChildNodeAdded -= OnChildAdded;
				this.actionTree.ChildNodeRemoved -= OnChildRemoved;
			}
			
			this.actionTree = actionTree;
			actionTree.ChildNodeAdded += OnChildAdded;
			actionTree.ChildNodeRemoved += OnChildRemoved;
			
			menuItems.Clear ();
			Widget wrapper = Widget.Lookup (this);
			
			foreach (Gtk.Widget w in Children) {
				Remove (w);
				w.Dispose ();
			}

			foreach (ActionTreeNode node in actionTree.Children) {
				ActionMenuItem aitem = new ActionMenuItem (wrapper, this, node);
				AddItem (aitem, -1);
				menuItems.Add (aitem);
			}
			
			if (showPlaceholder) {
				Gtk.Label emptyLabel = new Gtk.Label ();
				emptyLabel.Xalign = 0;
				emptyLabel.Markup = "<i><span foreground='darkgrey'>Click to create menu</span></i>";
				Gtk.MenuItem mit = new Gtk.MenuItem ();
				mit.Child = emptyLabel;
				mit.ButtonPressEvent += OnNewItemPress;
				Insert (mit, -1);
				mit.ShowAll ();
			}
		}
		
		void AddItem (ActionMenuItem aitem, int pos)
		{
			Gtk.Table t = new Gtk.Table (1, 3, false);
			aitem.Attach (t, 0, 0);
			aitem.KeyPressEvent += OnItemKeyPress;
			t.ShowAll ();
			
			CustomMenuBarItem it = new CustomMenuBarItem ();
			it.ActionMenuItem = aitem;
			aitem.Bind (it);
			it.Child = t;
			it.ShowAll ();
			Insert (it, pos);
		}
		
		public bool ShowInsertPlaceholder {
			get { return showPlaceholder; }
			set { showPlaceholder = value; Refresh (); }
		}
		
		void OnChildAdded (object ob, ActionTreeNodeArgs args)
		{
			Refresh ();
		}
		
		void OnChildRemoved (object ob, ActionTreeNodeArgs args)
		{
			Refresh ();
		}
		
		void Refresh ()
		{
			Widget wrapper = Widget.Lookup (this);
			IDesignArea area = wrapper.GetDesignArea ();
			ActionTreeNode selNode = null;
			
			foreach (Gtk.Widget w in Children) {
				CustomMenuBarItem it = w as CustomMenuBarItem;
				if (it != null && area.IsSelected (it.ActionMenuItem)) {
					selNode = it.ActionMenuItem.Node;
					area.ResetSelection (it.ActionMenuItem);
				}
				Remove (w);
			}
			
			FillMenu (actionTree);
			
			ActionMenuItem mi = FindMenuItem (selNode);
			if (mi != null)
				mi.Select ();
		}
		
		[GLib.ConnectBeforeAttribute]
		void OnNewItemPress (object ob, Gtk.ButtonPressEventArgs args)
		{
			InsertAction (menuItems.Count);
			args.RetVal = true;
		}
		
		void InsertAction (int pos)
		{
			Widget wrapper = Widget.Lookup (this);
			Action ac = (Action) ObjectWrapper.Create (wrapper.Project, new Gtk.Action ("", "", null, null));
			ActionTreeNode node = new ActionTreeNode (Gtk.UIManagerItemType.Menu, "", ac);
			actionTree.Children.Insert (pos, node);

			ActionMenuItem aitem = FindMenuItem (node);
			aitem.EditingDone -= OnEditingDone;
			aitem.Select ();
			aitem.StartEditing ();
		}
		
		void OnEditingDone (object ob, EventArgs args)
		{
			ActionMenuItem item = (ActionMenuItem) ob;
			item.EditingDone -= OnEditingDone;
			Widget wrapper = Widget.Lookup (this);
			
			if (item.Node.Action.GtkAction.Label.Length == 0) {
				IDesignArea area = wrapper.GetDesignArea ();
				area.ResetSelection (item);
				actionTree.Children.Remove (item.Node);
			} else {
				wrapper.LocalActionGroup.Actions.Add (item.Node.Action);
			}
		}
		
		public void Select (ActionTreeNode node)
		{
			ActionMenuItem item = FindMenuItem (node);
			if (item != null)
				item.Select ();
		}
		
		public void DropMenu (ActionTreeNode node)
		{
			ActionMenuItem item = FindMenuItem (node);
			if (item != null) {
				if (item.HasSubmenu) {
					item.ShowSubmenu ();
					if (openSubmenu != null)
						openSubmenu.Select (null);
				}
				else
					item.Select ();
			}
		}
		
		public ActionMenu OpenSubmenu {
			get { return openSubmenu; }
			set {
				if (openSubmenu != null) {
					openSubmenu.OpenSubmenu = null;
					Widget wrapper = Widget.Lookup (this);
					IDesignArea area = wrapper.GetDesignArea ();
					if (area != null)
						area.RemoveWidget (openSubmenu);
					openSubmenu.Dispose ();
				}
				openSubmenu = value;
			}
		}

		bool IMenuItemContainer.IsTopMenu { 
			get { return true; } 
		}
		
		Gtk.Widget IMenuItemContainer.Widget { 
			get { return this; }
		}
		
		protected override bool OnDragMotion (Gdk.DragContext context, int x, int y, uint time)
		{
			ActionPaletteItem dragItem = DND.DragWidget as ActionPaletteItem;
			if (dragItem == null)
				return false;
			
			if (actionTree.Children.Count > 0) {
				ActionMenuItem item = LocateWidget (x, y);
				if (item != null) {
					Widget wrapper = Widget.Lookup (this);
				
					// Show the submenu to allow droping to it, but avoid
					// droping a submenu inside itself
					if (item.HasSubmenu && item.Node != dragItem.Node)
						item.ShowSubmenu (wrapper.GetDesignArea(), item);
					
					// Look for the index where to insert the new item
					dropIndex = actionTree.Children.IndexOf (item.Node);
					int mpos = item.Allocation.X + item.Allocation.Width / 2;
					if (x > mpos)
						dropIndex++;
					
					// Calculate the drop position, used to show the drop bar
					if (dropIndex == 0)
						dropPosition = item.Allocation.X;
					else if (dropIndex == menuItems.Count)
						dropPosition = item.Allocation.Right;
					else {
						item = (ActionMenuItem) menuItems [dropIndex];
						ActionMenuItem prevItem = (ActionMenuItem) menuItems [dropIndex - 1];
						dropPosition = prevItem.Allocation.Right + (item.Allocation.X - prevItem.Allocation.Right)/2;
					}
				}
			} else
				dropIndex = 0;

			QueueDraw ();
			return base.OnDragMotion (context, x, y, time);
		}
		
		protected override void OnDragLeave (Gdk.DragContext context, uint time)
		{
			dropPosition = -1;
			QueueDraw ();
			base.OnDragLeave (context, time);
		}
		
		protected override bool OnDragDrop (Gdk.DragContext context, int x,	int y, uint time)
		{
			ActionPaletteItem dropped = DND.Drop (context, null, time) as ActionPaletteItem;
			if (dropped == null)
				return false;

			if (dropped.Node.ParentNode != null) {
				if (dropIndex < actionTree.Children.Count) {
					// Do nothing if trying to drop the node over the same node
					ActionTreeNode dropNode = actionTree.Children [dropIndex];
					if (dropNode == dropped.Node)
						return false;
					dropped.Node.ParentNode.Children.Remove (dropped.Node);
					
					// The drop position may have changed after removing the dropped node,
					// so get it again.
					dropIndex = actionTree.Children.IndexOf (dropNode);
					actionTree.Children.Insert (dropIndex, dropped.Node);
				} else {
					dropped.Node.ParentNode.Children.Remove (dropped.Node);
					actionTree.Children.Add (dropped.Node);
					dropIndex = actionTree.Children.Count - 1;
				}
			}
			
			// Select the dropped node
			ActionMenuItem mi = (ActionMenuItem) menuItems [dropIndex];
			mi.Select ();
			
			return base.OnDragDrop (context, x,	y, time);
		}		
		protected override bool OnExposeEvent (Gdk.EventExpose ev)
		{
			bool r = base.OnExposeEvent (ev);
			int w, h;
			this.GdkWindow.GetSize (out w, out h);
			if (dropPosition != -1)
				GdkWindow.DrawRectangle (this.Style.BlackGC, true, dropPosition, 0, 3, h);
			return r;
		}
		
		void OnItemKeyPress (object s, Gtk.KeyPressEventArgs args)
		{
			int pos = menuItems.IndexOf (s);
			ActionMenuItem item = (ActionMenuItem) s;
			
			switch (args.Event.Key) {
				case Gdk.Key.Left:
					if (pos > 0)
						((ActionMenuItem)menuItems[pos - 1]).Select ();
					break;
				case Gdk.Key.Right:
					if (pos < menuItems.Count - 1)
						((ActionMenuItem)menuItems[pos + 1]).Select ();
					else if (pos == menuItems.Count - 1)
						InsertAction (menuItems.Count);
					break;
				case Gdk.Key.Down:
					if (item.HasSubmenu) {
						item.ShowSubmenu ();
						if (openSubmenu != null)
							openSubmenu.Select (null);
					}
					break;
				case Gdk.Key.Up:
					OpenSubmenu = null;
					break;
			}
			args.RetVal = true;
		}
		
		ActionMenuItem LocateWidget (int x, int y)
		{
			foreach (ActionMenuItem mi in menuItems) {
				if (mi.Allocation.Contains (x, y))
					return mi;
			}
			return null;
		}
		
		ActionMenuItem FindMenuItem (ActionTreeNode node)
		{
			foreach (ActionMenuItem mi in menuItems) {
				if (mi.Node == node)
					return mi;
			}
			return null;
		}
	}
}