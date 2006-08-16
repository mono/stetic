
using System;
using System.Collections;
using Stetic.Wrapper;
using Mono.Unix;

namespace Stetic.Editor
{
	public class ActionMenu: Gtk.EventBox, IMenuItemContainer
	{
		ActionTreeNode parentNode;
		ActionTreeNodeCollection nodes;
		ArrayList menuItems = new ArrayList ();
		Gtk.Table table;
		ActionMenu openSubmenu;
		Widget wrapper;
		int dropPosition = -1;
		int dropIndex;
		Gtk.EventBox emptyLabel;
		IMenuItemContainer parentMenu;
		
		public ActionMenu (IntPtr p): base (p)
		{}
		
		internal ActionMenu (Widget wrapper, IMenuItemContainer parentMenu, ActionTreeNode node)
		{
			DND.DestSet (this, true);
			parentNode = node;
			this.parentMenu = parentMenu;
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
			foreach (Gtk.Widget w in table.Children) {
				table.Remove (w);
				w.Destroy ();
			}
			
			base.Dispose ();
			parentNode.ChildNodeAdded -= OnChildAdded;
			parentNode.ChildNodeRemoved -= OnChildRemoved;
		}
		
		public void Select (ActionTreeNode node)
		{
			if (node != null) {
				ActionMenuItem item = FindMenuItem (node);
				if (item != null)
					item.Select ();
			} else {
				if (menuItems.Count > 0)
					((ActionMenuItem)menuItems [0]).Select ();
			}
		}
		
		public ActionTreeNode ParentNode {
			get { return parentNode; }
		}
		
		bool IMenuItemContainer.IsTopMenu { 
			get { return false; } 
		}
		
		Gtk.Widget IMenuItemContainer.Widget { 
			get { return this; }
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
					item.KeyPressEvent += OnItemKeyPress;
					item.Attach (table, n++, 0);
					menuItems.Add (item);
				}
			}
			
			emptyLabel = new Gtk.EventBox ();
			emptyLabel.VisibleWindow = false;
			Gtk.Label label = new Gtk.Label ();
			label.Xalign = 0;
			label.Markup = "<i><span foreground='darkgrey'>" + Catalog.GetString ("Click to create action") + "</span></i>";
			emptyLabel.Add (label);
			emptyLabel.ButtonPressEvent += OnAddClicked;
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
			emptyLabel.Hide ();
		}
		
		void OnEditingDone (object ob, EventArgs args)
		{
			ActionMenuItem item = (ActionMenuItem) ob;
			item.EditingDone -= OnEditingDone;
			if (item.Node.Action.GtkAction.Label.Length == 0 && item.Node.Action.GtkAction.StockId == null) {
				IDesignArea area = wrapper.GetDesignArea ();
				area.ResetSelection (item);
				nodes.Remove (item.Node);
			} else {
				if (wrapper.LocalActionGroups.Count == 0)
					wrapper.LocalActionGroups.Add (new ActionGroup ("Default"));
				wrapper.LocalActionGroups [0].Actions.Add (item.Node.Action);
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
			return true;
		}
		
		void OnAddClicked (object s, Gtk.ButtonPressEventArgs args)
		{
			InsertAction (menuItems.Count);
			args.RetVal = true;
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

			if (dropped.Node.Type != Gtk.UIManagerItemType.Menuitem && 
				dropped.Node.Type != Gtk.UIManagerItemType.Menu &&
				dropped.Node.Type != Gtk.UIManagerItemType.Toolitem &&
				dropped.Node.Type != Gtk.UIManagerItemType.Separator)
				return false;
			
			ActionTreeNode newNode = null;
			
			// Toolitems are copied, not moved
			
			if (dropped.Node.ParentNode != null && dropped.Node.Type != Gtk.UIManagerItemType.Toolitem) {
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
			} else {
				newNode = new ActionTreeNode (Gtk.UIManagerItemType.Menuitem, null, dropped.Node.Action);
				nodes.Insert (dropIndex, newNode);
			}
			
			// Select the dropped node
			ActionMenuItem mi = (ActionMenuItem) menuItems [dropIndex];
			mi.Select ();
			
			return base.OnDragDrop (context, x,	y, time);
		}
		
		void OnItemKeyPress (object s, Gtk.KeyPressEventArgs args)
		{
			int pos = menuItems.IndexOf (s);
			ActionMenuItem item = (ActionMenuItem) s;
			
			switch (args.Event.Key) {
				case Gdk.Key.Up:
					if (pos > 0)
						((ActionMenuItem)menuItems[pos - 1]).Select ();
					else if (parentMenu.Widget is ActionMenuBar) {
						ActionMenuBar bar = (ActionMenuBar) parentMenu.Widget;
						bar.Select (parentNode);
					}
					break;
				case Gdk.Key.Down:
					if (pos < menuItems.Count - 1)
						((ActionMenuItem)menuItems[pos + 1]).Select ();
					else if (pos == menuItems.Count - 1) {
						InsertAction (menuItems.Count);
					}
					break;
				case Gdk.Key.Right:
					if (item.HasSubmenu) {
						item.ShowSubmenu ();
						if (openSubmenu != null)
							openSubmenu.Select (null);
					} else if (parentNode != null) {
						ActionMenuBar parentMB = parentMenu.Widget as ActionMenuBar;
						if (parentMB != null) {
							int i = parentNode.ParentNode.Children.IndexOf (parentNode);
							if (i < parentNode.ParentNode.Children.Count - 1)
								parentMB.DropMenu (parentNode.ParentNode.Children [i + 1]);
						}
					}
					break;
				case Gdk.Key.Left:
					if (parentNode != null) {
						ActionMenu parentAM = parentMenu.Widget as ActionMenu;
						if (parentAM != null) {
							parentAM.Select (parentNode);
						}
						ActionMenuBar parentMB = parentMenu.Widget as ActionMenuBar;
						if (parentMB != null) {
							int i = parentNode.ParentNode.Children.IndexOf (parentNode);
							if (i > 0)
								parentMB.DropMenu (parentNode.ParentNode.Children [i - 1]);
						}
					}
					break;
			}
			args.RetVal = true;
		}
		
		void InsertActionAt (ActionMenuItem item, bool after, bool separator)
		{
			int pos = menuItems.IndexOf (item);
			if (pos == -1)
				return;
			
			if (after)
				pos++;

			if (separator) {
				ActionTreeNode newNode = new ActionTreeNode (Gtk.UIManagerItemType.Separator, null, null);
				nodes.Insert (pos, newNode);
			} else
				InsertAction (pos);
		}
		
		void Paste (ActionMenuItem item)
		{
		}
		
		public void ShowContextMenu (ActionMenuItem menuItem)
		{
			Gtk.Menu m = new Gtk.Menu ();
			Gtk.MenuItem item = new Gtk.MenuItem (Catalog.GetString ("Insert Before"));
			m.Add (item);
			item.Activated += delegate (object s, EventArgs a) {
				InsertActionAt (menuItem, false, false);
			};
			item = new Gtk.MenuItem (Catalog.GetString ("Insert After"));
			m.Add (item);
			item.Activated += delegate (object s, EventArgs a) {
				InsertActionAt (menuItem, true, false);
			};
			item = new Gtk.MenuItem (Catalog.GetString ("Insert Separator Before"));
			m.Add (item);
			item.Activated += delegate (object s, EventArgs a) {
				InsertActionAt (menuItem, false, true);
			};
			item = new Gtk.MenuItem (Catalog.GetString ("Insert Separator After"));
			m.Add (item);
			item.Activated += delegate (object s, EventArgs a) {
				InsertActionAt (menuItem, true, true);
			};
			
			m.Add (new Gtk.SeparatorMenuItem ());
			
			item = new Gtk.ImageMenuItem (Gtk.Stock.Cut, null);
			m.Add (item);
			item.Activated += delegate (object s, EventArgs a) {
				menuItem.Cut ();
			};
			item.Visible = false;	// No copy & paste for now
			item = new Gtk.ImageMenuItem (Gtk.Stock.Copy, null);
			m.Add (item);
			item.Activated += delegate (object s, EventArgs a) {
				menuItem.Copy ();
			};
			item.Visible = false;
			item = new Gtk.ImageMenuItem (Gtk.Stock.Paste, null);
			m.Add (item);
			item.Activated += delegate (object s, EventArgs a) {
				Paste (menuItem);
			};
			item.Visible = false;
			
			item = new Gtk.ImageMenuItem (Gtk.Stock.Delete, null);
			m.Add (item);
			item.Activated += delegate (object s, EventArgs a) {
				menuItem.Delete ();
			};
			m.ShowAll ();
			m.Popup ();
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
	
	interface IMenuItemContainer
	{
		ActionMenu OpenSubmenu { get; set; }
		bool IsTopMenu { get; }
		Gtk.Widget Widget { get; }
		void ShowContextMenu (ActionMenuItem item);
	}
}
