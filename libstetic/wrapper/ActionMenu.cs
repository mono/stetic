
using System;
using System.Collections;

namespace Stetic.Wrapper
{
	public class ActionMenu: Gtk.EventBox
	{
		ActionTreeNode parentNode;
		ActionTreeNodeCollection nodes;
		ArrayList menuItems = new ArrayList ();
		Gtk.Table table;
		ActionMenu openSubmenu;
		Widget wrapper;
		int dropPosition = -1;
		int dropIndex;
		Gtk.Label emptyLabel;
		
		public ActionMenu (IntPtr p): base (p)
		{}
		
		public ActionMenu (Widget wrapper, ActionTreeNode node)
		{
			DND.DestSet (this, true);
			parentNode = node;
			this.wrapper = wrapper;
			this.nodes = node.Children;
			table = new Gtk.Table (0, 0, false);
			table.ColumnSpacing = 5;
			table.RowSpacing = 5;
			table.BorderWidth = 5;
			
			Add (table);
			
			Fill ();
			
			parentNode.ChildNodeAdded += OnChildAdded;
			parentNode.ChildNodeRemoved += OnChildRemoved;
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			parentNode.ChildNodeAdded -= OnChildAdded;
			parentNode.ChildNodeRemoved -= OnChildRemoved;
		}
		
		public ActionTreeNode ParentNode {
			get { return parentNode; }
		}
		
		public void TrackWidgetPosition (Gtk.Widget refWidget, bool topMenu)
		{
			IDesignArea area = wrapper.GetDesignArea ();
			Gdk.Rectangle rect = area.GetCoordinates (refWidget);
			if (topMenu)
				area.MoveWidget (this, rect.X, rect.Bottom);
			else
				area.MoveWidget (this, rect.Right, rect.Top);

			GLib.Timeout.Add (50, new GLib.TimeoutHandler (RepositionSubmenu));
		}
		
		public bool RepositionSubmenu ()
		{
			if (openSubmenu == null)
				return false;

			ActionMenuItem item = FindMenuItem (openSubmenu.parentNode);
			if (item != null)
				openSubmenu.TrackWidgetPosition (item, false);
			return false;
		}
		
		void Fill ()
		{
			menuItems.Clear ();

			uint n = 0;
			
			if (nodes.Count > 0) {
				foreach (ActionTreeNode node in nodes) {
					ActionMenuItem item = new ActionMenuItem (wrapper, this, node);
					item.Attach (table, n++);
					menuItems.Add (item);
				}
			}
			
			emptyLabel = new Gtk.Label ();
			emptyLabel.Xalign = 0;
			emptyLabel.Markup = "<i><span foreground='grey'>Click to create action</span></i>";
			table.Attach (emptyLabel, 1, 2, n, n + 1);
			
			ShowAll ();
		}
		
		void Refresh ()
		{
			IDesignArea area = wrapper.GetDesignArea ();
			ActionTreeNode selNode = null;
			
			foreach (Gtk.Widget w in table.Children) {
				if (area.IsSelected (w) && w is ActionMenuItem) {
					selNode = ((ActionMenuItem)w).Node;
					area.ResetSelection (w);
				}
				table.Remove (w);
			}
			
			Fill ();
			
			ActionMenuItem mi = FindMenuItem (selNode);
			if (mi != null)
				mi.Select ();

			GLib.Timeout.Add (50, new GLib.TimeoutHandler (RepositionSubmenu));
		}
		
		public ActionMenu OpenSubmenu {
			get { return openSubmenu; }
			set {
				if (openSubmenu != null) {
					openSubmenu.OpenSubmenu = null;
					IDesignArea area = wrapper.GetDesignArea ();
					area.RemoveWidget (openSubmenu);
					openSubmenu.Dispose ();
				}
				openSubmenu = value;
			}
		}
		
		void InsertAction (int pos)
		{
			Action ac = (Action) ObjectWrapper.Create (wrapper.Project, new Gtk.Action ("", "", null, null));
			ActionTreeNode newNode = new ActionTreeNode (Gtk.UIManagerItemType.Menuitem, null, ac);
			nodes.Insert (pos, newNode);
			ActionMenuItem item = FindMenuItem (newNode);
			item.EditingDone += OnEditingDone;
			item.Select ();
			item.StartEditing ();
		}
		
		void OnEditingDone (object ob, EventArgs args)
		{
			ActionMenuItem item = (ActionMenuItem) ob;
			item.EditingDone -= OnEditingDone;
			if (item.Node.Action.GtkAction.Label.Length == 0) {
				IDesignArea area = wrapper.GetDesignArea ();
				area.ResetSelection (item);
				nodes.Remove (item.Node);
			} else {
				wrapper.LocalActionGroup.Actions.Add (item.Node.Action);
			}
		}
		
		void OnChildAdded (object ob, ActionTreeNodeArgs args)
		{
			Refresh ();
		}
		
		void OnChildRemoved (object ob, ActionTreeNodeArgs args)
		{
			ActionMenuItem mi = FindMenuItem (args.Node);
			if (mi != null) {
				// Remove the table row that contains the menu item
				Gtk.Table.TableChild tc = (Gtk.Table.TableChild) table [mi];
				uint row = tc.TopAttach;
				mi.Detach ();
				menuItems.Remove (mi);
				foreach (Gtk.Widget w in table.Children) {
					tc = (Gtk.Table.TableChild) table [w];
					if (tc.TopAttach >= row)
						tc.TopAttach--;
					if (tc.BottomAttach > row)
						tc.BottomAttach--;
				}
//				RepositionSubmenu ();
				GLib.Timeout.Add (50, new GLib.TimeoutHandler (RepositionSubmenu));
			}
		}
		
		protected override bool OnExposeEvent (Gdk.EventExpose ev)
		{
			bool r = base.OnExposeEvent (ev);
			int w, h;
			this.GdkWindow.GetSize (out w, out h);
			Gdk.Rectangle clip = new Gdk.Rectangle (0,0,w,h);
			Gtk.Style.PaintBox (this.Style, this.GdkWindow, Gtk.StateType.Normal, Gtk.ShadowType.Out, clip, this, "menu", 0, 0, w, h);
			
			if (dropPosition != -1) {
				GdkWindow.DrawRectangle (this.Style.BlackGC, true, 0, dropPosition - 1, w - 2, 3);
			}
			
			return r;
		}
		
		protected override bool OnButtonPressEvent (Gdk.EventButton ev)
		{
			if (menuItems.Count > 0) {
				ActionMenuItem mi = (ActionMenuItem) menuItems [menuItems.Count - 1];
				if (mi.Allocation.Top < ev.Y)
					InsertAction (menuItems.Count);
			} else
				InsertAction (0);
			
			return true;
		}

		protected override bool OnDragMotion (Gdk.DragContext context, int x, int y, uint time)
		{
			ActionPaletteItem dragItem = DND.DragWidget as ActionPaletteItem;
			if (dragItem == null)
				return false;
			
			if (nodes.Count > 0) {
				ActionMenuItem item = LocateWidget (x, y);
				if (item != null) {
				
					// Show the submenu to allow droping to it, but avoid
					// droping a submenu inside itself
					if (item.HasSubmenu && item.Node != dragItem.Node)
						item.ShowSubmenu (wrapper.GetDesignArea(), item);
					
					// Look for the index where to insert the new item
					dropIndex = nodes.IndexOf (item.Node);
					int mpos = item.Allocation.Y + item.Allocation.Height / 2;
					if (y > mpos)
						dropIndex++;
					
					// Calculate the drop position, used to show the drop bar
					if (dropIndex == 0)
						dropPosition = item.Allocation.Y;
					else if (dropIndex == menuItems.Count)
						dropPosition = item.Allocation.Bottom;
					else {
						item = (ActionMenuItem) menuItems [dropIndex];
						ActionMenuItem prevItem = (ActionMenuItem) menuItems [dropIndex - 1];
						dropPosition = prevItem.Allocation.Bottom + (item.Allocation.Y - prevItem.Allocation.Bottom)/2;
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
		
		protected override void OnDragDataReceived (Gdk.DragContext context, int x, int y, Gtk.SelectionData data, uint info, uint time)
		{
		}
		
		protected override bool OnDragDrop (Gdk.DragContext context, int x,	int y, uint time)
		{
			ActionPaletteItem dropped = DND.Drop (context, null, time) as ActionPaletteItem;
			if (dropped == null)
				return false;

			ActionTreeNode newNode = null;
			
			if (dropped.Node.ParentNode != null) {
				if (dropIndex < nodes.Count) {
					// Do nothing if trying to drop the node over the same node
					ActionTreeNode dropNode = nodes [dropIndex];
					if (dropNode == dropped.Node)
						return false;
					dropped.Node.ParentNode.Children.Remove (dropped.Node);
					
					// The drop position may have changed after removing the dropped node,
					// so get it again.
					dropIndex = nodes.IndexOf (dropNode);
					nodes.Insert (dropIndex, dropped.Node);
				} else {
					dropped.Node.ParentNode.Children.Remove (dropped.Node);
					nodes.Add (dropped.Node);
					dropIndex = nodes.Count - 1;
				}
			} else if (dropped.Node.Action == null && dropped.Node.Type == Gtk.UIManagerItemType.Menuitem) {
				Gtk.Action ac = new Gtk.Action ("newAction", "New Action");
				Action wac = (Action) ObjectWrapper.Create (wrapper.Project, ac);
				newNode = new ActionTreeNode (Gtk.UIManagerItemType.Menuitem, null, wac);
				nodes.Insert (dropIndex, newNode);
			} else if (dropped.Node.Action == null && dropped.Node.Type == Gtk.UIManagerItemType.Menu) {
				Gtk.Action ac = new Gtk.Action ("newMenu", "New Menu");
				Action wac = (Action) ObjectWrapper.Create (wrapper.Project, ac);
				newNode = new ActionTreeNode (Gtk.UIManagerItemType.Menu, null, wac);
				nodes.Insert (dropIndex, newNode);
			} else if (dropped.Node.Type == Gtk.UIManagerItemType.Menuitem) {
				newNode = new ActionTreeNode (Gtk.UIManagerItemType.Menuitem, null, dropped.Node.Action);
				nodes.Insert (dropIndex, newNode);
			}
			
			// Select the dropped node
			ActionMenuItem mi = (ActionMenuItem) menuItems [dropIndex];
			mi.Select ();
			
			return base.OnDragDrop (context, x,	y, time);
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
