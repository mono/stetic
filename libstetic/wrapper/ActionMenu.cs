
using System;
using System.Collections;

namespace Stetic.Wrapper
{
	public class ActionMenu: Gtk.EventBox
	{
		ActionTreeNodeCollection nodes;
		ArrayList menuItems = new ArrayList ();
		Gtk.Table table;
		ActionMenu openSubmenu;
		Widget wrapper;
		int dropPosition = -1;
		int dropIndex;
		Gtk.Label emptyLabel;
		
		public ActionMenu (Widget wrapper, ActionTreeNodeCollection nodes)
		{
			DND.DestSet (this, true);
			this.wrapper = wrapper;
			this.nodes = nodes;
			table = new Gtk.Table (0, 0, false);
			table.ColumnSpacing = 5;
			table.RowSpacing = 5;
			table.BorderWidth = 5;
			
			Add (table);
			
			Fill ();
		}
		
		void Fill ()
		{
			menuItems.Clear ();
			if (emptyLabel != null) {
				table.Remove (emptyLabel);
				emptyLabel = null;
			}

			if (nodes.Count > 0) {
				uint n = 0;
				foreach (ActionTreeNode node in nodes) {
					ActionMenuItem item = new ActionMenuItem (wrapper, this, node);
					item.Attach (table, n++);
					menuItems.Add (item);
				}
			} else {
				emptyLabel = new Gtk.Label ();
				emptyLabel.Markup = " <i><span foreground='grey'>Empty</span></i> ";
				table.Attach (emptyLabel, 0, 1, 0, 1);
			}
			
			ShowAll ();
		}
		
		void Refresh ()
		{
			foreach (Gtk.Widget w in table.Children)
				table.Remove (w);
			Fill ();
		}
		
		public ActionMenu OpenSubmenu {
			get { return openSubmenu; }
			set {
				if (openSubmenu != null) {
					openSubmenu.OpenSubmenu = null;
					IDesignArea area = wrapper.GetDesignArea ();
					area.RemoveWidget (openSubmenu);
				}
				openSubmenu = value;
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
				GdkWindow.DrawRectangle (this.Style.BlackGC, true, 0, dropPosition - 1, w, 3);
			}
			
			return r;
		}
		
		protected override bool OnButtonPressEvent (Gdk.EventButton ev)
		{
			IDesignArea area = wrapper.GetDesignArea ();
			if (area != null)
				area.SetSelection (this, this);
			return true;
		}

		protected override bool OnDragMotion (Gdk.DragContext context, int x, int y, uint time)
		{
			if (!(DND.DragWidget is ActionPaletteItem))
				return false;
			
			if (nodes.Count > 0) {
				ActionMenuItem item = LocateWidget (x, y);
				if (item != null) {
					dropIndex = nodes.IndexOf (item.Node);
					int mpos = item.Allocation.Y + item.Allocation.Height / 2;
					if (y <= mpos)
						dropPosition = item.Allocation.Y;
					else {
						dropPosition = item.Allocation.Y + item.Allocation.Height;
						dropIndex++;
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
			
			if (dropped.Node.Action == null && dropped.Node.Type == Gtk.UIManagerItemType.Menuitem) {
				Gtk.Action ac = new Gtk.Action ("newAction", "New Action");
				Action wac = (Action) ObjectWrapper.Create (wrapper.Project, ac);
				newNode = new ActionTreeNode (Gtk.UIManagerItemType.Menuitem, null, wac);
				nodes.Insert (dropIndex, newNode);
			} else if (dropped.Node.Action == null && dropped.Node.Type == Gtk.UIManagerItemType.Menu) {
				Gtk.Action ac = new Gtk.Action ("newMenu", "New Menu");
				Action wac = (Action) ObjectWrapper.Create (wrapper.Project, ac);
				newNode = new ActionTreeNode (Gtk.UIManagerItemType.Menu, null, wac);
				nodes.Insert (dropIndex, newNode);
			}
			
			// Refresh the list and select the dropped node
			if (newNode != null) {
				Refresh ();
				ActionMenuItem mi = (ActionMenuItem) menuItems [dropIndex];
				IDesignArea area = wrapper.GetDesignArea ();
				if (area != null)
					area.SetSelection (mi, mi.Node.Action.GtkAction);
			}
			
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
	}
}
