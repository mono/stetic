
using System;

namespace Stetic.Wrapper
{
	public class ActionMenuItem: Gtk.EventBox
	{
		ActionTreeNode node;
		Widget wrapper;
		object parentMenu;
		
		Gtk.Widget icon;
		Gtk.Widget label;
		Gtk.Widget accel;
		int row;
		bool editing;
		bool localUpdate;
		static Gdk.Pixbuf addMenuImage;
		static Gdk.Pixbuf removeMenuImage;
		
		static ActionMenuItem ()
		{
			addMenuImage = Gdk.Pixbuf.LoadFromResource ("add-menu.png");
			removeMenuImage = Gdk.Pixbuf.LoadFromResource ("remove-menu.png");
		}
		
		public ActionMenuItem (Widget wrapper, object parent, ActionTreeNode node)
		{
			this.parentMenu = parent;
			this.wrapper = wrapper;
			this.node = node;
			if (node.Action != null)
				node.Action.ObjectChanged += OnActionChanged;
			CreateControls ();
			this.VisibleWindow = false;
		}
		
		public ActionTreeNode Node {
			get { return node; }
		}
		
		public void StartEditing ()
		{
			if (!editing) {
				editing = true;
				Refresh ();
				if (node.Type == Gtk.UIManagerItemType.Menu)
					HideSubmenu ();
			}
		}
		
		public void EndEditing ()
		{
			if (editing) {
				editing = false;
				Refresh ();
				while (Gtk.Application.EventsPending ())
					Gtk.Application.RunIteration ();
				if (node.Type == Gtk.UIManagerItemType.Menu) {
					IDesignArea area = wrapper.GetDesignArea ();
					if (area != null)
						ShowSubmenu (area, this);
				}
			}
		}
		
		public void Attach (Gtk.Table table, uint row)
		{
			AttachChildren (table, row);
			table.Attach (this, 0, 3, row, row + 1);
		}
		
		void AttachChildren (Gtk.Table table, uint row)
		{
			if (icon != null)
				table.Attach (icon, 0, 1, row, row + 1);
			if (label != null)
				table.Attach (label, 1, 2, row, row + 1);
			if (accel != null)
				table.Attach (accel, 2, 3, row, row + 1);
		}
		
		void CreateControls ()
		{
			if (node.Action == null)
				return;

			Gtk.Action gaction = node.Action.GtkAction;
		
			string text = gaction.Label;
			string stock = gaction.StockId;

			if (node.Action.Type == Action.ActionType.Radio) {
				icon = new CheckActionIcon (true, node.Action.Active);
			} else if (node.Action.Type == Action.ActionType.Toggle) {
				icon = new CheckActionIcon (node.Action.DrawAsRadio, node.Action.Active);
			}
				
			if (stock != null) {
				Gtk.StockItem item = Gtk.Stock.Lookup (stock);
				if (text == null || text.Length == 0)
					text = item.Label;
				
				if (item.Keyval != 0 && !editing) {
					Gtk.Label lac = new Gtk.Label ();
					string accelName =  Gtk.Accelerator.Name (item.Keyval, item.Modifier).ToUpper ();
					accelName = accelName.Replace ("<CONTROL>", "Ctrl+");
					accelName = accelName.Replace ("<SHIFT>", "Shift+");
					accelName = accelName.Replace ("<ALT>", "Alt+");
					lac.Text = accelName;
					accel = lac;
				}
				if (icon == null)
					icon = node.Action.GtkAction.CreateIcon (Gtk.IconSize.Menu);
			}
			
			if (editing) {
				Gtk.Tooltips tooltips = new Gtk.Tooltips ();
				
				Gtk.HBox bbox = new Gtk.HBox ();
				if (icon != null) {
					bbox.PackStart (icon, false, false, 0);
				}
				bbox.PackStart (new Gtk.Arrow (Gtk.ArrowType.Down, Gtk.ShadowType.In), false, false, 0);
				Gtk.Button b = new Gtk.Button (bbox);
				tooltips.SetTip (b, "Select action type", "");
				b.Relief = Gtk.ReliefStyle.None;
				b.ButtonPressEvent += OnSelectIcon;
				icon = b;
				
				Gtk.Entry entry = new Gtk.Entry ();
				entry.Text = text;
				entry.Changed += OnLabelChanged;
				entry.Activated += OnLabelActivated;
				entry.HasFrame = false;
				entry.GrabFocus ();
				this.label = entry;
				
				// Add a button for creating / deleting a submenu
				Gdk.Pixbuf img;
				string tip;
				if (node.Type != Gtk.UIManagerItemType.Menu) {
					img = addMenuImage;
					tip = "Add submenu";
				} else {
					img = removeMenuImage;
					tip = "Remove submenu";
				}
					
				Gtk.Button sb = new Gtk.Button (new Gtk.Image (img));
				tooltips.SetTip (sb, tip, "");
				sb.Relief = Gtk.ReliefStyle.None;
				sb.Clicked += OnCreateDeleteSubmenu;
				
				// Make sure the button is alligned to the right of the column
				bbox = new Gtk.HBox ();
				bbox.PackEnd (sb, false, false, 0);
				accel = bbox;
				
			} else {
				Gtk.Label label = new Gtk.Label (text);
				label.Xalign = 0;
				this.label = label;
			}
			
			if (node.Type == Gtk.UIManagerItemType.Menu && !editing) {
				Gtk.Arrow arrow = new Gtk.Arrow (Gtk.ArrowType.Right, Gtk.ShadowType.None);
				arrow.Xalign = 1;
				this.accel = arrow;
			}
		}
		
		void OnLabelChanged (object ob, EventArgs args)
		{
			localUpdate = true;
			
			Gtk.Entry entry = ob as Gtk.Entry;
			if (entry.Text.Length > 0) {
				if (node.Action == null) {
					Gtk.Action ac = new Gtk.Action (entry.Text, entry.Text);
					Action wac = (Action) ObjectWrapper.Create (wrapper.Project, ac);
					node.Action = wac;
				}
					
				node.Action.GtkAction.Label = entry.Text;
				node.Action.NotifyChanged ();
			}
			localUpdate = false;
		}
		
		void OnCreateDeleteSubmenu (object ob, EventArgs args)
		{
			if (node.Type == Gtk.UIManagerItemType.Menu) {
				node.Type = Gtk.UIManagerItemType.Menuitem;
				node.Children.Clear ();
			} else {
				node.Type = Gtk.UIManagerItemType.Menu;
			}
			
			EndEditing ();
			node.Action.NotifyChanged ();
		}
		
		void OnLabelActivated (object ob, EventArgs args)
		{
			EndEditing ();
		}
		
		[GLib.ConnectBeforeAttribute]
		void OnSelectIcon (object sender, Gtk.ButtonPressEventArgs e)
		{
			Gtk.Menu menu = new Gtk.Menu ();
			Gtk.HBox box = new Gtk.HBox ();
			box.PackStart (new CheckActionIcon (false, true), false, false, 0);
			box.PackStart (new Gtk.Label ("Toggle"), false, false, 3);
			Gtk.MenuItem it = new Gtk.MenuItem ();
			it.Child = box;
			it.Activated += OnSetToggleType;
			menu.Insert (it, -1);
			
			box = new Gtk.HBox ();
			box.PackStart (new CheckActionIcon (true, true), false, false, 0);
			box.PackStart (new Gtk.Label ("Radio"), false, false, 3);
			it = new Gtk.MenuItem ();
			it.Child = box;
			it.Activated += OnSetRadioType;
			menu.Insert (it, -1);
			
			it = new Gtk.MenuItem ("Action");
			it.Activated += OnSetActionType;
			menu.Insert (it, -1);
			
			it = new Gtk.MenuItem ("Stock Action...");
			it.Activated += OnSetStockActionType;
			menu.Insert (it, -1);
			
			menu.ShowAll ();
			menu.Popup (null, null, new Gtk.MenuPositionFunc (OnDropMenuPosition), 3, Gtk.Global.CurrentEventTime);
			e.RetVal = false;
		}
		
		void OnDropMenuPosition (Gtk.Menu menu, out int x, out int y, out bool pushIn)
		{
			this.ParentWindow.GetOrigin (out x, out y);
			x += this.Allocation.X;
			y += this.Allocation.Y + this.Allocation.Height;
			pushIn = true;
		}
		
		void OnSetToggleType (object ob, EventArgs args)
		{
			node.Action.Type = Action.ActionType.Toggle;
			node.Action.NotifyChanged ();
		}
		
		void OnSetRadioType (object ob, EventArgs args)
		{
			node.Action.Type = Action.ActionType.Radio;
			node.Action.NotifyChanged ();
		}
		
		void OnSetActionType (object ob, EventArgs args)
		{
			node.Action.Type = Action.ActionType.Action;
			node.Action.NotifyChanged ();
		}
		
		void OnSetStockActionType (object ob, EventArgs args)
		{
			Stetic.Editor.SelectImageDialog dialog = new Stetic.Editor.SelectImageDialog (null, wrapper.Project);
			using (dialog)
			{
				if (dialog.Run () != (int) Gtk.ResponseType.Ok)
					return;

				node.Action.Type = Action.ActionType.Action;
				node.Action.GtkAction.StockId = dialog.Icon.Name;
				node.Action.NotifyChanged ();
			}
			
		}
		
		public void Refresh ()
		{
			Gtk.Table table = (Gtk.Table)Parent;
			if (table == null)
				return;
			
			if (icon != null)
				table.Remove (icon);
			if (label != null)
				table.Remove (label);
			if (accel != null)
				table.Remove (accel);

			icon = label = accel = null;
			CreateControls ();
			AttachChildren (table, ((Gtk.Table.TableChild)table[this]).TopAttach);
			table.ShowAll ();
		}
		
		protected override bool OnButtonPressEvent (Gdk.EventButton ev)
		{
			IDesignArea area = wrapper.GetDesignArea ();
			if (area == null)
				return true;

			// Clicking a selected item starts the edit mode
			if (area.IsSelected (this)) {
				StartEditing ();
				return true;
			}
			
			IObjectSelection sel = area.SetSelection (this, node.Action.GtkAction);
			sel.Drag += HandleItemDrag;
			sel.Disposed += OnSelectionDisposed;
			
			if (parentMenu is ActionMenu)
				((ActionMenu)parentMenu).OpenSubmenu = null;
			else
				((MenuBar)parentMenu).OpenSubmenu = null;
				
			if (node.Type == Gtk.UIManagerItemType.Menu)
				ShowSubmenu (area, this);
			return true;
		}
		
		void OnActionChanged (object ob, ObjectWrapperEventArgs a)
		{
			if (!localUpdate)
				Refresh ();
		}
		
		void OnSelectionDisposed (object ob, EventArgs a)
		{
			EndEditing ();
		}
		
		public void ShowSubmenu (IDesignArea area, Gtk.Widget refWidget)
		{
			Gdk.Rectangle rect = area.GetCoordinates (refWidget);
			ActionMenu menu = new ActionMenu (wrapper, node.Children);
			menu.ShowAll ();
			if (parentMenu is ActionMenu) {
				area.AddWidget (menu, rect.Right, rect.Top);
				((ActionMenu)parentMenu).OpenSubmenu = menu;
			} else if (parentMenu is MenuBar) {
				area.AddWidget (menu, rect.Left, rect.Bottom);
				((MenuBar)parentMenu).OpenSubmenu = menu;
			}
		}
		
		void HideSubmenu ()
		{
			if (parentMenu is ActionMenu) {
				((ActionMenu)parentMenu).OpenSubmenu = null;
			} else if (parentMenu is MenuBar) {
				((MenuBar)parentMenu).OpenSubmenu = null;
			}
		}
		
		void HandleItemDrag (Gdk.EventMotion evt)
		{
/*			Gtk.Widget dragWidget = selection;
			dragSource = CreateDragSource (dragWidget);
			DND.Drag (dragSource, evt, dragWidget);
*/		}
	}
	
	class CheckActionIcon: Gtk.EventBox
	{
		readonly bool isRadio;
		readonly bool active;
		
		public CheckActionIcon (bool isRadio, bool active)
		{
			this.isRadio = isRadio;
			this.active = active;
			WidthRequest = HeightRequest = 16;
		}
		
		protected override bool OnExposeEvent (Gdk.EventExpose ev)
		{
			Gdk.Rectangle rect = Allocation;
			rect.X = rect.Y = 0;
			
			Gtk.ShadowType sh = active ? Gtk.ShadowType.In : Gtk.ShadowType.Out;
			if (isRadio)
				Gtk.Style.PaintOption (this.Style, this.GdkWindow, this.State, sh, rect, this, "", rect.X, rect.Y, rect.Width, rect.Height);
			else
				Gtk.Style.PaintCheck (this.Style, this.GdkWindow, this.State, sh, rect, this, "", rect.X, rect.Y, rect.Width, rect.Height);
			return true;
		}
	}
}
