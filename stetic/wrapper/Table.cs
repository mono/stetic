using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Table", "table.png", WidgetType.Container)]
	public class Table : Gtk.Table, Stetic.IContainerWrapper, Stetic.IContextMenuProvider {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup TableProperties;

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		public static PropertyGroup TableChildProperties;

		static Table () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Stetic.Wrapper.Table), typeof (Gtk.Table), "NRows"),
				new PropertyDescriptor (typeof (Stetic.Wrapper.Table), typeof (Gtk.Table), "NColumns"),
				new PropertyDescriptor (typeof (Gtk.Table), "Homogeneous"),
				new PropertyDescriptor (typeof (Gtk.Table), "RowSpacing"),
				new PropertyDescriptor (typeof (Gtk.Table), "ColumnSpacing"),
				new PropertyDescriptor (typeof (Gtk.Container), "BorderWidth"),
			};				
			TableProperties = new PropertyGroup ("Table Properties", props);

			groups = new PropertyGroup[] {
				TableProperties,
				Widget.CommonWidgetProperties
			};

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Table.TableChild), "TopAttach"),
				new PropertyDescriptor (typeof (Gtk.Table.TableChild), "BottomAttach"),
				new PropertyDescriptor (typeof (Gtk.Table.TableChild), "LeftAttach"),
				new PropertyDescriptor (typeof (Gtk.Table.TableChild), "RightAttach"),
				new PropertyDescriptor (typeof (Gtk.Table.TableChild), "XPadding"),
				new PropertyDescriptor (typeof (Gtk.Table.TableChild), "YPadding"),
				new PropertyDescriptor (typeof (Stetic.Wrapper.Table.TableChild), "AutoSize"),
				new PropertyDescriptor (typeof (Gtk.Table.TableChild), "XOptions"),
				new PropertyDescriptor (typeof (Gtk.Table.TableChild), "YOptions")
			};				
			TableChildProperties = new PropertyGroup ("Table Child Layout", props);

			childgroups = new PropertyGroup[] {
				TableChildProperties
			};
		}

		const AttachOptions expandOpts = AttachOptions.Expand | AttachOptions.Fill;
		const AttachOptions fillOpts = AttachOptions.Fill;

		public Table () : base (3, 3, false)
		{
			Sync ();
		}

		public new class TableChild : Gtk.Table.TableChild, Stetic.IPropertySensitizer {
			protected internal TableChild (Gtk.Container parent, Gtk.Widget child) : base (parent, child)
			{
				autosize = (child is WidgetSite);
			}

			bool autosize;

			public bool AutoSize {
				get {
					return autosize;
				}
				set {
					autosize = value;
					if (SensitivityChanged != null) {
						SensitivityChanged ("XOptions", !autosize);
						SensitivityChanged ("YOptions", !autosize);
					}
					((Stetic.Wrapper.Table)parent).Sync ();
				}
			}

			public IEnumerable InsensitiveProperties {
				get {
					if (autosize)
						return new string[] { "XOptions", "YOptions" };
					else
						return new string[0];
				}
			}

			public event SensitivityChangedDelegate SensitivityChanged;
		}

		Hashtable children = new Hashtable ();
		public override Gtk.Container.ContainerChild this [Gtk.Widget child] {
			get {
				return children[child] as Gtk.Container.ContainerChild;
			}
		}

		int freeze;
		void Freeze ()
		{
			freeze++;
		}

		void Thaw ()
		{
			if (--freeze == 0)
				Sync ();
		}

		void Sync ()
		{
			uint left, right, top, bottom;
			uint row, col;
			WidgetSite site;
			WidgetSite[,] grid;
			TableChild tc;
			Gtk.Widget[] children;

			if (freeze > 0)
				return;
			freeze = 1;

			children = Children;

			grid = new WidgetSite[NRows,NColumns];

			// First fill in the placeholders in the grid. If we find any
			// placeholders covering more than one grid square, remove them.
			// (New ones will be created below.)
                        foreach (Gtk.Widget child in children) {
				site = (WidgetSite) child;
				if (site.Occupied)
					continue;

                                tc = this[child] as TableChild;
                                left = tc.LeftAttach;
                                right = tc.RightAttach;
                                top = tc.TopAttach;
                                bottom = tc.BottomAttach;

				if (right == left + 1 && bottom == top + 1)
					grid[top,left] = child as WidgetSite;
				else
					Remove (child);
                        }

			// Now fill in the real widgets, knocking out any placeholders
			// they overlap.
                        foreach (Gtk.Widget child in children) {
				site = (WidgetSite) child;
				if (!site.Occupied)
					continue;

                                tc = this[child] as TableChild;
                                left = tc.LeftAttach;
                                right = tc.RightAttach;
                                top = tc.TopAttach;
                                bottom = tc.BottomAttach;

                                for (row = top; row < bottom; row++) {
                                        for (col = left; col < right; col++) {
						if (grid[row,col] != null)
							Remove (grid[row,col]);
                                                grid[row,col] = child as WidgetSite;
                                        }
                                }
                        }

			// Scan each row; if there are any empty cells, fill them in
			// with placeholders. If a row contains only placeholders, then
			// set them all to expand vertically so the row won't collapse.
			// OTOH, if the row contains any real widget, set any placeholders
			// in that row to not expand vertically, so they don't force the
			// real widgets to expand further than they should. If any row
			// is vertically expandable, then the table as a whole is.
			vexpandable = false;
			for (row = 0; row < NRows; row++) {
				bool allPlaceholders = true;

				for (col = 0; col < NColumns; col++) {
					if (grid[row,col] == null) {
						site = new WidgetSite ();
						site.OccupancyChanged += SiteOccupancyChanged;
						site.ChildNotified += ChildNotification;
						site.Show ();
						Attach (site, col, col + 1, row, row + 1);
						this.children[site] = new TableChild (this, site);
						grid[row,col] = site;
					} else if (!grid[row,col].VExpandable)
						allPlaceholders = false;
				}

				for (col = 0; col < NColumns; col++) {
					site = grid[row,col];
					tc = this[site] as TableChild;
					if (!tc.AutoSize)
						continue;
					tc.YOptions = allPlaceholders ? expandOpts : fillOpts;
				}

				if (allPlaceholders)
					vexpandable = true;
			}

			// Now do the same for columns and horizontal expansion (but we
			// don't have to worry about empty cells this time).
			hexpandable = false;
			for (col = 0; col < NColumns; col++) {
				bool allPlaceholders = true;

				for (row = 0; row < NRows; row++) {
					if (!grid[row,col].HExpandable) {
						allPlaceholders = false;
						break;
					}
				}

				for (row = 0; row < NRows; row++) {
					site = grid[row,col];
					tc = this[site] as TableChild;
					if (!tc.AutoSize)
						continue;
					tc.XOptions = allPlaceholders ? expandOpts : fillOpts;
				}

				if (allPlaceholders)
					hexpandable = true;
			}

			freeze = 0;

			if (ExpandabilityChanged != null)
				ExpandabilityChanged (this);
		}

		public new uint NRows {
			get {
				return base.NRows;
			}
			set {
				Freeze ();
				while (value < base.NRows)
					DeleteRow (base.NRows);
				base.NRows = value;
				Thaw ();
			}
		}

		public new uint NColumns {
			get {
				return base.NColumns;
			}
			set {
				Freeze ();
				while (value < base.NColumns)
					DeleteColumn (base.NColumns);
				base.NColumns = value;
				Thaw ();
			}
		}

		void AddRow (uint row)
		{
			Freeze ();
			base.NRows++;
			foreach (Gtk.Widget w in Children) {
				Table.TableChild tc = this[w] as Table.TableChild;

				if (tc.BottomAttach > row)
					tc.BottomAttach++;
				if (tc.TopAttach >= row)
					tc.TopAttach++;
			}
			Thaw ();
		}

		void DeleteRow (uint row)
		{
			Gtk.Widget[] children = Children;
			Table.TableChild tc;

			Freeze ();
			foreach (Gtk.Widget child in children) {
				tc = this[child] as Table.TableChild;

				if (tc.TopAttach == row) {
					if (tc.BottomAttach == tc.TopAttach + 1)
						Remove (child);
					else
						tc.BottomAttach--;
				} else {
					if (tc.TopAttach > row)
						tc.TopAttach--;
					if (tc.BottomAttach > row)
						tc.BottomAttach--;
				}
			}
			base.NRows--;
			Thaw ();
		}

		void AddColumn (uint col)
		{
			Freeze ();
			base.NColumns++;
			foreach (Gtk.Widget w in Children) {
				Table.TableChild tc = this[w] as Table.TableChild;

				if (tc.RightAttach > col)
					tc.RightAttach++;
				if (tc.LeftAttach >= col)
					tc.LeftAttach++;
			}
			Thaw ();
		}

		void DeleteColumn (uint col)
		{
			Gtk.Widget[] children = Children;
			Table.TableChild tc;

			Freeze ();
			foreach (Gtk.Widget child in children) {
				tc = this[child] as Table.TableChild;

				if (tc.LeftAttach == col) {
					if (tc.RightAttach == tc.LeftAttach + 1)
						Remove (child);
					else
						tc.RightAttach--;
				} else {
					if (tc.LeftAttach > col)
						tc.LeftAttach--;
					if (tc.RightAttach > col)
						tc.RightAttach--;
				}
			}
			base.NColumns--;
			Thaw ();
		}

		public IEnumerable ContextMenuItems (IWidgetSite context)
		{
			ContextMenuItem[] items;

			// FIXME; I'm only assigning to a variable rather than
			// returning it directly to make emacs indentation happy
			items = new ContextMenuItem[] {
				new ContextMenuItem ("Insert Row Before", new ContextMenuItemDelegate (InsertRowBefore)),
				new ContextMenuItem ("Insert Row After", new ContextMenuItemDelegate (InsertRowAfter)),
				new ContextMenuItem ("Insert Column Before", new ContextMenuItemDelegate (InsertColumnBefore)),
				new ContextMenuItem ("Insert Column After", new ContextMenuItemDelegate (InsertColumnAfter)),
				new ContextMenuItem ("Delete Row", new ContextMenuItemDelegate (DeleteRow)),
				new ContextMenuItem ("Delete Column", new ContextMenuItemDelegate (DeleteColumn)),
			};
			return items;
		}

		void InsertRowBefore (IWidgetSite context)
		{
			Table.TableChild tc = this[(Gtk.Widget)context] as Table.TableChild;
			AddRow (tc.TopAttach);
		}

		void InsertRowAfter (IWidgetSite context)
		{
			Table.TableChild tc = this[(Gtk.Widget)context] as Table.TableChild;
			AddRow (tc.BottomAttach);
		}

		void InsertColumnBefore (IWidgetSite context)
		{
			Table.TableChild tc = this[(Gtk.Widget)context] as Table.TableChild;
			AddColumn (tc.LeftAttach);
		}

		void InsertColumnAfter (IWidgetSite context)
		{
			Table.TableChild tc = this[(Gtk.Widget)context] as Table.TableChild;
			AddColumn (tc.RightAttach);
		}

		void DeleteRow (IWidgetSite context)
		{
			Table.TableChild tc = this[(Gtk.Widget)context] as Table.TableChild;
			DeleteRow (tc.TopAttach);
		}

		void DeleteColumn (IWidgetSite context)
		{
			Table.TableChild tc = this[(Gtk.Widget)context] as Table.TableChild;
			DeleteColumn (tc.LeftAttach);
		}

		private bool hexpandable, vexpandable;
		public bool HExpandable { get { return hexpandable; } }
		public bool VExpandable { get { return vexpandable; } }

		public event ExpandabilityChangedHandler ExpandabilityChanged;

		protected override void OnRemoved (Gtk.Widget w)
		{
			WidgetSite site = w as WidgetSite;

			if (site == null)
				return;

			site.OccupancyChanged -= SiteOccupancyChanged;
			site.ChildNotified -= ChildNotification;
			children.Remove (site);

			base.OnRemoved (w);
		}

		private void SiteOccupancyChanged (WidgetSite isite)
		{
			WidgetSite site = (WidgetSite)isite;

			Freeze ();
			if (site.Occupied) {
				Table.TableChild tc = this[site] as Table.TableChild;
				tc.XOptions = 0;
				tc.YOptions = 0;
			}
			Thaw ();
		}

		private void ChildNotification (object o, ChildNotifiedArgs args)
		{
//			if (!(args.Pspec is ParamSpecUInt))
//				return;

			Sync ();
		}

		private const int delta = 4;
		private enum Where { None, Above, Below, Left, Right };

		protected override bool OnButtonPressEvent (Gdk.EventButton evt)
		{
			int diff, closest;
			Where where = Where.None;
			Rectangle alloc;

			if (evt.Type != EventType.TwoButtonPress)
				return false;

			if (FocusChild == null)
				return false;

			alloc = FocusChild.Allocation;

			closest = delta;
			diff = (int)evt.X;
			if (diff < closest) {
				closest = diff;
				where = Where.Left;
			}
			diff = alloc.Width - (int)evt.X;
			if (diff < closest) {
				closest = diff;
				where = Where.Right;
			}
			diff = (int)evt.Y;
			if (diff < closest) {
				closest = diff;
				where = Where.Above;
			}
			diff = alloc.Height - (int)evt.Y;
			if (diff < closest) {
				closest = diff;
				where = Where.Below;
			}

			Table.TableChild tc = this[FocusChild] as Table.TableChild;

			switch (where) {
			case Where.None:
				return false;

			case Where.Left:
				AddColumn (tc.LeftAttach);
				break;

			case Where.Right:
				AddColumn (tc.RightAttach);
				break;

			case Where.Above:
				AddRow (tc.TopAttach);
				break;

			case Where.Below:
				AddRow (tc.BottomAttach);
				break;
			}

			return true;
		}
	}
}
