
using System;
using Stetic.Wrapper;

namespace Stetic.Editor
{
	public class ActionMenuItem: Gtk.EventBox
	{
		ActionTreeNode node;
		Widget wrapper;
		IMenuItemContainer parentMenu;
		
		Gtk.Widget icon;
		Gtk.Widget label;
		Gtk.Widget accel;
		bool editing;
		bool localUpdate;
		bool editOnRelease;
		bool motionDrag;
		CustomMenuBarItem menuBarItem;
		uint itemSpacing;
		int minWidth;
		
		static Gdk.Pixbuf addMenuImage;
		static Gdk.Pixbuf removeMenuImage;
		
		// To use in the action editor
		IDesignArea designArea;
		IProject project;
		
		public event EventHandler EditingDone;
		
		static ActionMenuItem ()
		{
			addMenuImage = Gdk.Pixbuf.LoadFromResource ("add-menu.png");
			removeMenuImage = Gdk.Pixbuf.LoadFromResource ("remove-menu.png");
		}
		
		internal ActionMenuItem (Widget wrapper, IMenuItemContainer parent, ActionTreeNode node)
		: this (wrapper, parent, node, 0)
		{
		}
		
		internal ActionMenuItem (IDesignArea designArea, IProject project, IMenuItemContainer parent, ActionTreeNode node)
		: this (null, parent, node, 6)
		{
			this.project = project;
			this.designArea = designArea;
		}
		
		internal ActionMenuItem (Widget wrapper, IMenuItemContainer parent, ActionTreeNode node, uint itemSpacing)
		{
			DND.SourceSet (this);
			this.parentMenu = parent;
			this.wrapper = wrapper;
			this.node = node;
			if (node.Action != null)
				node.Action.ObjectChanged += OnActionChanged;
			this.VisibleWindow = false;
			this.CanFocus = true;
			this.Events |= Gdk.EventMask.KeyPressMask;
			this.itemSpacing = itemSpacing;
			CreateControls ();
		}
		
		public override void Dispose ()
		{
			base.Dispose ();
			if (menuBarItem != null) {
				menuBarItem.ButtonPressEvent -= OnMenuItemPress;
				menuBarItem.ButtonReleaseEvent -= OnMemuItemRelease;
				menuBarItem.MotionNotifyEvent -= OnMotionNotify;
			}
		}
		
		public ActionTreeNode Node {
			get { return node; }
		}
		
		public bool HasSubmenu {
			get { return node.Type == Gtk.UIManagerItemType.Menu; }
		}
		
		public uint ItemSpacing {
			get { return itemSpacing; }
			set { itemSpacing = value; }
		}
		
		public int MinWidth {
			get { return minWidth; }
			set { minWidth = value; }
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
					if (wrapper != null) {
						IDesignArea area = wrapper.GetDesignArea ();
						if (area != null)
							ShowSubmenu (area, this);
					}
				}
				GrabFocus ();
				if (EditingDone != null)
					EditingDone (this, EventArgs.Empty);
			}
		}
		
		public void Select ()
		{
			IDesignArea area = GetDesignArea ();
			if (area.IsSelected (this))
				return;
			IObjectSelection sel = area.SetSelection (this, node.Action != null ? node.Action.GtkAction : null);
			sel.Drag += HandleItemDrag;
			sel.Disposed += OnSelectionDisposed;
			
			parentMenu.OpenSubmenu = null;
				
			if (HasSubmenu)
				ShowSubmenu (area, this);
			GrabFocus ();
		}
		
		public void Copy ()
		{
		}
		
		public void Cut ()
		{
		}
		
		public void Delete ()
		{
			if (node.ParentNode != null)
				node.ParentNode.Children.Remove (node);
			Destroy ();
		}
		
		public void Attach (Gtk.Table table, uint row, uint col)
		{
			table.Attach (this, col, col + 3, row, row + 1);
			Show ();
			AttachChildren (table, row, col);
		}
		
		void AttachChildren (Gtk.Table table, uint row, uint col)
		{
			if (icon != null) {
				table.Attach (icon, col, col + 1, row, row + 1);
				Gtk.Table.TableChild tc = (Gtk.Table.TableChild) table [icon];
				if (!editing)
					tc.YPadding = itemSpacing;
			}
			if (label != null) {
				table.Attach (label, col + 1, col + 2, row, row + 1);
				Gtk.Table.TableChild tc = (Gtk.Table.TableChild) table [label];
				if (!editing)
					tc.YPadding = itemSpacing;
				label.GrabFocus ();
			}
			if (accel != null)
				table.Attach (accel, col + 2, col + 3, row, row + 1);

			if (minWidth > 0 && label != null) {
				if (label.SizeRequest().Width < minWidth)
					label.WidthRequest = minWidth;
			}
		}
		
		void CreateControls ()
		{
			if (node.Type == Gtk.UIManagerItemType.Separator) {
				Gtk.Widget sep;
				if (parentMenu.IsTopMenu) {
					sep = new Gtk.VSeparator ();
					sep.WidthRequest = 6;
				} else {
					sep = new Gtk.HSeparator ();
					sep.HeightRequest = 6;
				}
				Add (sep);
				ShowAll ();
				return;
			} else {
				if (Child != null && Child is Gtk.Separator)
					Remove (Child);
			}
			
			if (node.Action == null)
				return;
				
			bool isGlobal = wrapper != null && wrapper.Project.ActionGroups.IndexOf (node.Action.ActionGroup) != -1;

			Gtk.Action gaction = node.Action.GtkAction;
			bool barItem = parentMenu.IsTopMenu;
		
			string text = gaction.Label;
			string stock = gaction.StockId;

			if (barItem) {
				icon = null;
			} else if (node.Action.Type == Action.ActionType.Radio) {
				icon = new CheckActionIcon (true, node.Action.Active);
			} else if (node.Action.Type == Action.ActionType.Toggle) {
				icon = new CheckActionIcon (node.Action.DrawAsRadio, node.Action.Active);
			}
				
			if (stock != null) {
				Gtk.StockItem item = Gtk.Stock.Lookup (stock);
				if (text == null || text.Length == 0)
					text = item.Label;
				
				if (item.Keyval != 0 && !editing && !barItem) {
					Gtk.Label lac = new Gtk.Label ();
					string accelName =  Gtk.Accelerator.Name (item.Keyval, item.Modifier).ToUpper ();
					accelName = accelName.Replace ("<CONTROL>", "Ctrl+");
					accelName = accelName.Replace ("<SHIFT>", "Shift+");
					accelName = accelName.Replace ("<ALT>", "Alt+");
					lac.Text = accelName;
					accel = lac;
				}
				
				if (icon == null && !barItem)
					icon = node.Action.CreateIcon (Gtk.IconSize.Menu);
			}
			
			Gtk.Tooltips tooltips = null;
			if (editing)
				tooltips = new Gtk.Tooltips ();
			
			if (editing && !isGlobal) {
				if (!barItem) {
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
				} else
					icon = null;
				
				Gtk.Entry entry = new Gtk.Entry ();
				entry.Text = text;
				entry.Changed += OnLabelChanged;
				entry.Activated += OnLabelActivated;
				entry.HasFrame = false;
				this.label = entry;
				tooltips.SetTip (entry, "Action label", "");
			} else {
				Gtk.Label label = new Gtk.Label (text);
				label.Xalign = 0;
				this.label = label;
			}
			
			if (editing && wrapper != null) {
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
				Gtk.HBox bbox = new Gtk.HBox ();
				bbox.PackEnd (sb, false, false, 0);
				accel = bbox;
			}
			
			
			if (node.Type == Gtk.UIManagerItemType.Menu && !editing && !barItem) {
				Gtk.Arrow arrow = new Gtk.Arrow (Gtk.ArrowType.Right, Gtk.ShadowType.None);
				arrow.Xalign = 1;
				this.accel = arrow;
			}
			
			if (itemSpacing > 0) {
				// Add some padding to the left of the icon
				Gtk.Alignment a = new Gtk.Alignment (0, 0.5f, 0, 0);
				a.LeftPadding = itemSpacing;
				a.Add (icon);
				icon = a;
			}
		}
		
		public void Detach ()
		{
			Gtk.Table table = (Gtk.Table)Parent;
			if (icon != null)
				table.Remove (icon);
			if (label != null)
				table.Remove (label);
			if (accel != null)
				table.Remove (accel);
			table.Remove (this);
		}
		
		void OnLabelChanged (object ob, EventArgs args)
		{
			localUpdate = true;
			
			Gtk.Entry entry = ob as Gtk.Entry;
			if (entry.Text.Length > 0 || node.Action.GtkAction.StockId != null) {
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
			
			Gtk.CheckMenuItem item = new Gtk.CheckMenuItem ("Action");
			item.DrawAsRadio = true;
			item.Active = (node.Action.Type == Action.ActionType.Action);
			item.Activated += OnSetActionType;
			menu.Insert (item, -1);
			
			item = new Gtk.CheckMenuItem ("Radio Action");
			item.DrawAsRadio = true;
			item.Active = (node.Action.Type == Action.ActionType.Radio);
			item.Activated += OnSetRadioType;
			menu.Insert (item, -1);
			
			item = new Gtk.CheckMenuItem ("Toggle Action");
			item.DrawAsRadio = true;
			item.Active = (node.Action.Type == Action.ActionType.Toggle);
			item.Activated += OnSetToggleType;
			menu.Insert (item, -1);
			
			menu.Insert (new Gtk.SeparatorMenuItem (), -1);
			
			Gtk.MenuItem it = new Gtk.MenuItem ("Select Icon / Stock...");
			it.Sensitive = (node.Action.Type == Action.ActionType.Action);
			it.Activated += OnSetStockActionType;
			menu.Insert (it, -1);
			
			it = new Gtk.MenuItem ("Clear Icon");
			it.Sensitive = (node.Action.Type == Action.ActionType.Action);
			it.Activated += OnClearIcon;
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
			Stetic.Editor.SelectIconDialog dialog = new Stetic.Editor.SelectIconDialog (null, GetProject());
			using (dialog)
			{
				if (dialog.Run () != (int) Gtk.ResponseType.Ok)
					return;

				node.Action.Type = Action.ActionType.Action;
				node.Action.GtkAction.StockId = dialog.Icon;
				node.Action.NotifyChanged ();
			}
			
		}
		
		void OnClearIcon (object on, EventArgs args)
		{
			node.Action.GtkAction.StockId = null;
			node.Action.NotifyChanged ();
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
			Gtk.Table.TableChild tc = (Gtk.Table.TableChild)table[this];
			AttachChildren (table, tc.TopAttach, tc.LeftAttach);
			table.ShowAll ();
		}
		
		internal void Bind (CustomMenuBarItem item)
		{
			// When embedding the action menu in a MenuBar,
			// the parent menu item intercepts the mouse events,
			// so those events must be manually bound here
			menuBarItem = item;
			item.ButtonPressEvent += OnMenuItemPress;
			item.ButtonReleaseEvent += OnMemuItemRelease;
			item.MotionNotifyEvent += OnMotionNotify;
		}
		
		[GLib.ConnectBeforeAttribute]
		void OnMenuItemPress (object ob, Gtk.ButtonPressEventArgs args)
		{
			Gtk.Widget mit = (Gtk.Widget) ob;
			if (wrapper != null && wrapper.Project.Selection != mit.Parent) {
				wrapper.Select ();
				args.RetVal = true;
				return;
			}
			motionDrag = true;
			args.RetVal = ProcessButtonPress (args.Event);
		}
		
		[GLib.ConnectBeforeAttribute]
		void OnMemuItemRelease (object ob, Gtk.ButtonReleaseEventArgs args)
		{
			args.RetVal = ProcessButtonRelease (args.Event);
			motionDrag = false;
		}
		
		void OnMotionNotify (object ob, Gtk.MotionNotifyEventArgs args)
		{
			if (motionDrag) {
				// Looks like drag begin can be intercepted, so the motion notify
				// has to be used.
				ProcessDragBegin (null, args.Event);
				motionDrag = false;
			}
		}
		
		protected override bool OnButtonPressEvent (Gdk.EventButton ev)
		{
			return ProcessButtonPress (ev);
		}
		
		public bool ProcessButtonPress (Gdk.EventButton ev)
		{
			if (ev.Button == 1) {
				IDesignArea area = GetDesignArea ();
				if (area == null)
					return true;

				// Clicking a selected item starts the edit mode
				if (area.IsSelected (this)) {
					editOnRelease = true;
					return true;
				}
			} else if (ev.Button == 3) {
				parentMenu.ShowContextMenu (this);
			}
			
			Select ();
			return true;
		}
		
		protected override bool OnButtonReleaseEvent (Gdk.EventButton ev)
		{
			return ProcessButtonRelease (ev);
		}
		
		public bool ProcessButtonRelease (Gdk.EventButton ev)
		{
			// Clicking a selected item starts the edit mode
			if (editOnRelease)
				StartEditing ();

			editOnRelease = false;
			return true;
		}
		
		protected override void OnDragBegin (Gdk.DragContext ctx)
		{
			ProcessDragBegin (ctx, null);
		}
		
		public void ProcessDragBegin (Gdk.DragContext ctx, Gdk.EventMotion evt)
		{
			if (HasSubmenu)
				HideSubmenu ();
			editOnRelease = false;
			ActionPaletteItem item = new ActionPaletteItem (node);
			if (ctx != null)
				DND.Drag (parentMenu.Widget, ctx, item);
			else
				DND.Drag (parentMenu.Widget, evt, item);
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
		
		public void ShowSubmenu ()
		{
			ShowSubmenu (wrapper.GetDesignArea (), this);
		}
		
		public void ShowSubmenu (IDesignArea area, Gtk.Widget refWidget)
		{
			HideSubmenu ();
			Gdk.Rectangle rect = area.GetCoordinates (refWidget);
			ActionMenu menu = new ActionMenu (wrapper, parentMenu, node);
			menu.ShowAll ();
			area.AddWidget (menu, rect.Right, rect.Top);
			menu.TrackWidgetPosition (refWidget, parentMenu.IsTopMenu);
			
			parentMenu.OpenSubmenu = menu;
		}
		
		void HideSubmenu ()
		{
			parentMenu.OpenSubmenu = null;
		}
		
		void HandleItemDrag (Gdk.EventMotion evt)
		{
			ActionPaletteItem item = new ActionPaletteItem (node);
			DND.Drag (parentMenu.Widget, evt, item);
		}
		
		IDesignArea GetDesignArea ()
		{
			if (wrapper != null)
				return wrapper.GetDesignArea ();
			else
				return designArea;
		}
		
		IProject GetProject ()
		{
			if (wrapper != null)
				return wrapper.Project;
			else
				return project;
		}
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
