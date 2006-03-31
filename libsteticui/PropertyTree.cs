
using System;
using System.Collections;
using Gtk;
using Gdk;

namespace Stetic
{
	public class PropertyTree: Gtk.ScrolledWindow
	{
		Gtk.TreeStore store;
		InternalTree tree;
		TreeViewColumn editorColumn;
		Hashtable propertyRows;
		Hashtable sensitives, invisibles;
		TreeModelFilter filter;
		ArrayList expandStatus = new ArrayList ();
		
		public PropertyTree ()
		{
			propertyRows = new Hashtable ();
			sensitives = new Hashtable ();
			invisibles = new Hashtable ();
			
			store = new TreeStore (typeof (string), typeof(object), typeof(bool), typeof(object));
			
			TreeModelFilterVisibleFunc filterFunct = new TreeModelFilterVisibleFunc (FilterHiddenProperties); 
			filter = new TreeModelFilter (store, null);
            filter.VisibleFunc = filterFunct;
			
			tree = new InternalTree (filter);

			CellRendererText crt;
			
			TreeViewColumn col;

			col = new TreeViewColumn ();
			col.Title = "Property";
			crt = new CellRendererPropertyGroup (tree);
			col.PackStart (crt, true);
			col.SetCellDataFunc (crt, new TreeCellDataFunc (GroupData));
			col.Resizable = true;
			col.Expand = false;
			col.Sizing = TreeViewColumnSizing.Fixed;
			col.FixedWidth = 180;
			tree.AppendColumn (col);
			
			editorColumn = new TreeViewColumn ();
			editorColumn.Title = "Value";
			
			CellRendererProperty crp = new CellRendererProperty (tree);
			
			editorColumn.PackStart (crp, true);
			editorColumn.SetCellDataFunc (crp, new TreeCellDataFunc (PropertyData));
			editorColumn.Resizable = true;
			tree.AppendColumn (editorColumn);
			
			Add (tree);
			ShowAll ();
			
			tree.Selection.Changed += OnSelectionChanged;
		}
		
		public void AddProperties (ItemGroupCollection itemGroups, object instance)
		{
			foreach (ItemGroup igroup in itemGroups)
				AddGroup (igroup, instance);
		}
		
		public void SaveStatus ()
		{
			expandStatus.Clear ();

			TreeIter iter;
			if (!tree.Model.GetIterFirst (out iter))
				return;
			
			do {
				if (tree.GetRowExpanded (tree.Model.GetPath (iter))) {
					expandStatus.Add (tree.Model.GetValue (iter, 0));
				}
			} while (tree.Model.IterNext (ref iter));
		}
		
		public void RestoreStatus ()
		{
			TreeIter iter;
			if (!tree.Model.GetIterFirst (out iter))
				return;
			
			do {
				object grp = tree.Model.GetValue (iter, 0);
				if (expandStatus.Contains (grp))
					tree.ExpandRow (tree.Model.GetPath (iter), true);
			} while (tree.Model.IterNext (ref iter));
		}
		
		public virtual void Clear ()
		{
			store.Clear ();
			propertyRows.Clear ();
			sensitives.Clear ();
			invisibles.Clear ();
		}
		
		public virtual void Update ()
		{
			// Just repaint the cells
			QueueDraw ();
			
/*			if (tree.Editing) {
			
				TreeModel model;
				TreeIter iterSelected;
				tree.Selection.GetSelected (out model, out iterSelected);
				
				TreePath[] rows = tree.Selection.GetSelectedRows ();
				
//				string spath = model.GetStringFromIter (iterSelected);
				string spath = rows != null && rows.Length > 0 ? rows[0].ToString () : "";
				Console.WriteLine ("UPDATE " + spath + " " + model.GetPath (iterSelected));
				
				foreach (DictionaryEntry entry in propertyRows) {
					PropertyDescriptor prop = (PropertyDescriptor) entry.Key;
					string ppath = (string) entry.Value;
					if (prop.HasDependencies && spath != ppath) {
						TreeIter iter;
						Console.WriteLine ("  UPD " + prop.Name + " " + ppath + " " + prop.HasDependencies);
						store.GetIterFromString (out iter, ppath);
						store.EmitRowChanged (new TreePath (ppath), iter);
						break;
					}
				}
			} else {
				Console.WriteLine ("REFILTERING");
			//	filter.Refilter ();
			}
			
//			if (rows != null && rows.Length > 0) {
//				SetCursor (rows[0], GetColumn (1), true);
//			}
*/
		}
		
		public void AddGroup (ItemGroup igroup, object instance)
		{
			InstanceData idata = new InstanceData (instance);
			
			TreeIter iter = store.AppendValues (igroup.Label, null, true, idata);
			
			foreach (ItemDescriptor item in igroup) {
				if (item.IsInternal)
					continue;
				if (item is PropertyDescriptor)
					AppendProperty (iter, (PropertyDescriptor)item, idata);
				else if (item is CommandDescriptor)
					AppendCommand ((CommandDescriptor)item);
			}
		}
		
		protected void AppendProperty (PropertyDescriptor prop, object instance)
		{
			AppendProperty (TreeIter.Zero, prop, new InstanceData (instance));
		}
		
		protected void AppendProperty (TreeIter piter, PropertyDescriptor prop, object instance)
		{
			AppendProperty (piter, prop, new InstanceData (instance));
		}
		
		void AppendProperty (TreeIter piter, PropertyDescriptor prop, InstanceData idata)
		{
			TreeIter iter;
			if (piter.Equals (TreeIter.Zero))
				iter = store.AppendValues (prop.Label, prop, false, idata);
			else
				iter = store.AppendValues (piter, prop.Label, prop, false, idata);
			if (prop.HasDependencies)
				sensitives[prop] = prop;
			if (prop.HasVisibility)
				invisibles[prop] = prop;
			propertyRows [prop] = store.GetStringFromIter (iter);
		}
		
		protected void AppendCommand (CommandDescriptor prop)
		{
			// Ignore commands for now. They are added to the widget action bar
		}
		
		public void OnSelectionChanged (object s, EventArgs a)
		{
			TreePath[] rows = tree.Selection.GetSelectedRows ();
			if (rows != null && rows.Length > 0) {
				tree.SetCursor (rows[0], editorColumn, true);
			}
		}
		
		void PropertyData (Gtk.TreeViewColumn tree_column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			CellRendererProperty rc = (CellRendererProperty) cell;
			bool group = (bool) model.GetValue (iter, 2);
			if (group) {
				rc.SetData (null, null, null);
			} else {
				PropertyDescriptor prop = (PropertyDescriptor) model.GetValue (iter, 1);
				PropertyEditorCell propCell = PropertyEditorCell.GetPropertyCell (prop);
				InstanceData idata = (InstanceData) model.GetValue (iter, 3);
				propCell.Initialize (tree, prop, idata.Instance);
				rc.SetData (idata.Instance, prop, propCell);
			}
		}
		
		void GroupData (Gtk.TreeViewColumn tree_column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			CellRendererPropertyGroup rc = (CellRendererPropertyGroup) cell;
			rc.IsGroup = (bool) model.GetValue (iter, 2);
			rc.Text = (string) model.GetValue (iter, 0);
			
			PropertyDescriptor prop = (PropertyDescriptor) model.GetValue (iter, 1);
			if (prop != null) {
				InstanceData idata = (InstanceData) model.GetValue (iter, 3);
				rc.SensitiveProperty = prop.EnabledFor (idata.Instance);
			} else
				rc.SensitiveProperty = true;
		}

		bool FilterHiddenProperties (TreeModel model, TreeIter iter)
		{
			PropertyDescriptor prop = (PropertyDescriptor) model.GetValue (iter, 1);
			if (prop == null)
				return true;
			InstanceData idata = (InstanceData) model.GetValue (iter, 3);
			if (idata == null || idata.Instance == null)
				return true;
			
			return prop.VisibleFor (idata.Instance);
		}
	}
	
	class InternalTree: TreeView
	{
		internal ArrayList Groups = new ArrayList ();
		Pango.Layout layout;
		bool editing;
		
		public InternalTree (TreeModel model): base (model)
		{
	//		RulesHint = true;
			layout = new Pango.Layout (this.PangoContext);
			layout.Wrap = Pango.WrapMode.Char;
			Pango.FontDescription des = this.Style.FontDescription.Copy();
			layout.FontDescription = des;
		}
		
		public bool Editing {
			get { return editing; }
			set { editing = value; Update (); }
		}
		
		protected override bool OnExposeEvent (Gdk.EventExpose e)
		{
			Groups.Clear ();
			
			bool res = base.OnExposeEvent (e);
			
			foreach (TreeGroup grp in Groups) {
				layout.SetText (grp.Group);
				e.Window.DrawLayout (this.Style.TextGC (grp.State), grp.X, grp.Y, layout);
			}
			
			return res;
		}
		
		public virtual void Update ()
		{
			((TreeModelFilter)Model).Refilter ();
		}
	}
	
	class TreeGroup
	{
		public string Group;
		public int X;
		public int Y;
		public StateType State;
	}
	
	class CellRendererProperty: CellRenderer
	{
		PropertyDescriptor property;
		object instance;
		int rowHeight;
		Gdk.Color darkColor;
		PropertyEditorCell editorCell;
		bool sensitive;
		
		public CellRendererProperty (TreeView tree)
		{
			darkColor = tree.Style.Backgrounds [(int) Gtk.StateType.Normal];
			
			Xalign = 0;
			Xpad = 3;
			
			Mode |= Gtk.CellRendererMode.Editable;
			Entry dummyEntry = new Gtk.Entry ();
			rowHeight = dummyEntry.SizeRequest ().Height;
		}
		
		public void SetData (object instance, PropertyDescriptor property, PropertyEditorCell editor)
		{
			this.instance = instance;
			this.property = property;
			if (property == null)
				this.CellBackgroundGdk = darkColor;
			else
				this.CellBackground = null;
			
			sensitive = property != null ? property.EnabledFor (instance) : true;
			editorCell = editor;
		}

		public override void GetSize (Widget widget, ref Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			if (editorCell != null)
				editorCell.GetSize ((int)(cell_area.Width - this.Xpad * 2), out width, out height);
			else {
				width = height = 0;
			}
			
			width += (int) this.Xpad * 2;
			height += (int) this.Ypad * 2;

			x_offset = 0;
			y_offset = 0;
			
			if (height < rowHeight)
				height = rowHeight;
		}

		protected override void Render (Drawable window, Widget widget, Rectangle background_area, Rectangle cell_area, Rectangle expose_area, CellRendererState flags)
		{
			if (instance == null)
				return;
			int width = 0, height = 0;
			int iwidth = cell_area.Width - (int) this.Xpad * 2;
			
			if (editorCell != null)
				editorCell.GetSize ((int)(cell_area.Width - this.Xpad * 2), out width, out height);

			Rectangle bounds = new Rectangle ();
			bounds.Width = width > iwidth ? iwidth : width;
			bounds.Height = height;
			bounds.X = (int) (cell_area.X + this.Xpad);
			bounds.Y = cell_area.Y + (cell_area.Height - height) / 2;
			
			StateType state = GetState (flags);
				
			if (editorCell != null)
				editorCell.Render (window, bounds, state);
		}
		
		public override CellEditable StartEditing (Gdk.Event ev, Widget widget, string path, Gdk.Rectangle background_area, Gdk.Rectangle cell_area, CellRendererState flags)
		{
			if (property == null || editorCell == null || !sensitive)
				return null;

			StateType state = GetState (flags);
			EditSession session = editorCell.StartEditing (cell_area, state);
			if (session == null)
				return null;
			Gtk.Widget propEditor = (Gtk.Widget) session.Editor;
			propEditor.Show ();
			HackEntry e = new HackEntry (propEditor);
			e.Show ();
			return e;
		}
		
		StateType GetState (CellRendererState flags)
		{
			if (!sensitive)
				return StateType.Insensitive;
			else if ((flags & CellRendererState.Selected) != 0)
				return StateType.Selected;
			else
				return StateType.Normal;
		}
	}

	public class CellRendererPropertyGroup: CellRendererText
	{
		Pango.Layout layout;
		bool isGroup;
		Gdk.Color darkColor;
		bool sensitive;
		
		public bool IsGroup {
			get { return isGroup; }
			set { 
				isGroup = value;
				if (value)
					this.CellBackgroundGdk = darkColor;
				else
					this.CellBackground = null;
			}
		}
		
		public bool SensitiveProperty {
			get { return sensitive; }
			set { sensitive = value; }
		}
		
		public CellRendererPropertyGroup (TreeView tree)
		{
			layout = new Pango.Layout (tree.PangoContext);
			layout.Wrap = Pango.WrapMode.Char;
			darkColor = tree.Style.Backgrounds [(int) Gtk.StateType.Normal];
			
			Pango.FontDescription des = tree.Style.FontDescription.Copy();
			layout.FontDescription = des;
		}
		
		protected void GetCellSize (Widget widget, int availableWidth, out int width, out int height)
		{
			layout.SetMarkup (Text);
			layout.Width = -1;
			layout.GetPixelSize (out width, out height);
		}
		
		public override void GetSize (Widget widget, ref Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
		{
			GetCellSize (widget, (int)(cell_area.Width - this.Xpad * 2), out width, out height);
			width += (int) this.Xpad * 2;
			height += (int) this.Ypad * 2;
			
			x_offset = y_offset = 0;
			
			if (IsGroup)
				width = 0;
		}

		protected override void Render (Drawable window, Widget widget, Rectangle background_area, Rectangle cell_area, Rectangle expose_area, CellRendererState flags)
		{
			int width, height;
			GetCellSize (widget, (int)(cell_area.Width - this.Xpad * 2), out width, out height);

			int x = (int) (cell_area.X + this.Xpad);
			int y = cell_area.Y + (cell_area.Height - height) / 2;

			StateType state;
			if (!sensitive)
				state = StateType.Insensitive;
			else if ((flags & CellRendererState.Selected) != 0)
				state = StateType.Selected;
			else
				state = StateType.Normal;

			if (IsGroup) {
				TreeGroup grp = new TreeGroup ();
				grp.X = x;
				grp.Y = y;
				grp.Group = Text;
				grp.State = state;
				InternalTree tree = (InternalTree) widget;
				tree.Groups.Add (grp);
			} else {
				window.DrawLayout (widget.Style.TextGC (state), x, y, layout);
				int bx = background_area.X + background_area.Width - 1;
				Gdk.GC gc = new Gdk.GC (window);
		   		gc.RgbFgColor = darkColor;
				window.DrawLine (gc, bx, background_area.Y, bx, background_area.Y + background_area.Height);
			}
		}
	}

	class HackEntry: Entry
	{
		EventBox box;
		
		public HackEntry (Gtk.Widget child)
		{
			box = new EventBox ();
			box.ButtonPressEvent += new ButtonPressEventHandler (OnClickBox);
			box.ModifyBg (StateType.Normal, Style.White);
			box.Add (child);
		}
		
		[GLib.ConnectBefore]
		void OnClickBox (object s, ButtonPressEventArgs args)
		{
			// Avoid forwarding the button press event to the
			// tree, since it would hide the cell editor.
			args.RetVal = true;
		}
		
		protected override void OnParentSet (Gtk.Widget parent)
		{
			base.OnParentSet (parent);
			
			if (Parent != null) {
				if (this.ParentWindow != null)
					box.ParentWindow = this.ParentWindow;
				box.Parent = Parent;
				box.Show ();
				((InternalTree)Parent).Editing = true;
			}
			else {
				((InternalTree)parent).Editing = false;
				box.Unparent ();
			}
		}
		
		protected override void OnShown ()
		{
			// Do nothing.
		}
		
		protected override void OnSizeAllocated (Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			box.SizeRequest ();
			box.Allocation = allocation;
		}
	}
	
	class InstanceData 
	{
		public InstanceData (object instance) 
		{
			Instance = instance;
		}
		
		public object Instance;
	}
}
