using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Table", "table.png", ObjectWrapperType.Container)]
	public class Table : Stetic.Wrapper.Container {

		public static ItemGroup TableProperties;

		static Table () {
			TableProperties = new ItemGroup ("Table Properties",
							 typeof (Stetic.Wrapper.Table),
							 typeof (Gtk.Table),
							 "NRows",
							 "NColumns",
							 "Homogeneous",
							 "RowSpacing",
							 "ColumnSpacing",
							 "BorderWidth");
			RegisterWrapper (typeof (Stetic.Wrapper.Table),
					 TableProperties,
					 Widget.CommonWidgetProperties);

			ItemGroup contextMenu = new ItemGroup (null,
							       typeof (Stetic.Wrapper.Table),
							       typeof (Gtk.Table),
							       "InsertRowBefore",
							       "InsertRowAfter",
							       "InsertColumnBefore",
							       "InsertColumnAfter",
							       "DeleteRow",
							       "DeleteColumn");
			RegisterContextMenu (typeof (Stetic.Wrapper.Table), contextMenu);
		}

		const Gtk.AttachOptions expandOpts = Gtk.AttachOptions.Expand | Gtk.AttachOptions.Fill;
		const Gtk.AttachOptions fillOpts = Gtk.AttachOptions.Fill;

		public Table (IStetic stetic) : this (stetic, new Gtk.Table (3, 3, false)) {}

		public Table (IStetic stetic, Gtk.Table table) : base (stetic, table)
		{
			AutoSize = new Set ();
			Sync ();
		}

		private Gtk.Table table {
			get {
				return (Gtk.Table)Wrapped;
			}
		}

		Set AutoSize;

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
			Gtk.Table.TableChild tc;
			Gtk.Widget[] children;

			if (freeze > 0)
				return;
			freeze = 1;

			children = table.Children;
			grid = new WidgetSite[NRows,NColumns];

			foreach (Gtk.Widget child in children)
				child.FreezeChildNotify ();

			// First fill in the placeholders in the grid. If we find any
			// placeholders covering more than one grid square, remove them.
			// (New ones will be created below.)
                        foreach (Gtk.Widget child in children) {
				site = (WidgetSite) child;
				if (site.Occupied)
					continue;

                                tc = table[child] as Gtk.Table.TableChild;
                                left = tc.LeftAttach;
                                right = tc.RightAttach;
                                top = tc.TopAttach;
                                bottom = tc.BottomAttach;

				if (right == left + 1 && bottom == top + 1)
					grid[top,left] = child as WidgetSite;
				else
					table.Remove (child);
                        }

			// Now fill in the real widgets, knocking out any placeholders
			// they overlap.
                        foreach (Gtk.Widget child in children) {
				site = (WidgetSite) child;
				if (!site.Occupied)
					continue;

                                tc = table[child] as Gtk.Table.TableChild;
                                left = tc.LeftAttach;
                                right = tc.RightAttach;
                                top = tc.TopAttach;
                                bottom = tc.BottomAttach;

                                for (row = top; row < bottom; row++) {
                                        for (col = left; col < right; col++) {
						if (grid[row,col] != null)
							table.Remove (grid[row,col]);
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
						site = CreateWidgetSite ();
						site.FreezeChildNotify ();
						AutoSize[site] = true;
						table.Attach (site, col, col + 1, row, row + 1);
						grid[row,col] = site;
					} else if (!grid[row,col].VExpandable)
						allPlaceholders = false;
				}

				for (col = 0; col < NColumns; col++) {
					site = grid[row,col];
					if (!AutoSize[site])
						continue;
					tc = table[site] as Gtk.Table.TableChild;
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
					if (!AutoSize[site])
						continue;
					tc = table[site] as Gtk.Table.TableChild;
					tc.XOptions = allPlaceholders ? expandOpts : fillOpts;
				}

				if (allPlaceholders)
					hexpandable = true;
			}

                        foreach (Gtk.Widget child in table.Children)
				child.ThawChildNotify ();
			freeze = 0;

			EmitContentsChanged ();
		}

		public uint NRows {
			get {
				return table.NRows;
			}
			set {
				Freeze ();
				while (value < table.NRows)
					DeleteRow (table.NRows);
				table.NRows = value;
				Thaw ();
			}
		}

		public uint NColumns {
			get {
				return table.NColumns;
			}
			set {
				Freeze ();
				while (value < table.NColumns)
					DeleteColumn (table.NColumns);
				table.NColumns = value;
				Thaw ();
			}
		}

		void AddRow (uint row)
		{
			Freeze ();
			table.NRows++;
			foreach (Gtk.Widget w in table.Children) {
				Gtk.Table.TableChild tc = table[w] as Gtk.Table.TableChild;

				if (tc.BottomAttach > row)
					tc.BottomAttach++;
				if (tc.TopAttach >= row)
					tc.TopAttach++;
			}
			Thaw ();
		}

		void DeleteRow (uint row)
		{
			Gtk.Widget[] children = table.Children;
			Gtk.Table.TableChild tc;

			Freeze ();
			foreach (Gtk.Widget child in children) {
				tc = table[child] as Gtk.Table.TableChild;

				if (tc.TopAttach == row) {
					if (tc.BottomAttach == tc.TopAttach + 1)
						table.Remove (child);
					else
						tc.BottomAttach--;
				} else {
					if (tc.TopAttach > row)
						tc.TopAttach--;
					if (tc.BottomAttach > row)
						tc.BottomAttach--;
				}
			}
			table.NRows--;
			Thaw ();
		}

		void AddColumn (uint col)
		{
			Freeze ();
			table.NColumns++;
			foreach (Gtk.Widget w in table.Children) {
				Gtk.Table.TableChild tc = table[w] as Gtk.Table.TableChild;

				if (tc.RightAttach > col)
					tc.RightAttach++;
				if (tc.LeftAttach >= col)
					tc.LeftAttach++;
			}
			Thaw ();
		}

		void DeleteColumn (uint col)
		{
			Gtk.Widget[] children = table.Children;
			Gtk.Table.TableChild tc;

			Freeze ();
			foreach (Gtk.Widget child in children) {
				tc = table[child] as Gtk.Table.TableChild;

				if (tc.LeftAttach == col) {
					if (tc.RightAttach == tc.LeftAttach + 1)
						table.Remove (child);
					else
						tc.RightAttach--;
				} else {
					if (tc.LeftAttach > col)
						tc.LeftAttach--;
					if (tc.RightAttach > col)
						tc.RightAttach--;
				}
			}
			table.NColumns--;
			Thaw ();
		}

		[Command ("Insert Row Before", "Insert an empty row above the selected row")]
		void InsertRowBefore (IWidgetSite context)
		{
			Gtk.Table.TableChild tc = table[(Gtk.Widget)context] as Gtk.Table.TableChild;
			AddRow (tc.TopAttach);
		}

		[Command ("Insert Row After", "Insert an empty row below the selected row")]
		void InsertRowAfter (IWidgetSite context)
		{
			Gtk.Table.TableChild tc = table[(Gtk.Widget)context] as Gtk.Table.TableChild;
			AddRow (tc.BottomAttach);
		}

		[Command ("Insert Column Before", "Insert an empty column before the selected column")]
		void InsertColumnBefore (IWidgetSite context)
		{
			Gtk.Table.TableChild tc = table[(Gtk.Widget)context] as Gtk.Table.TableChild;
			AddColumn (tc.LeftAttach);
		}

		[Command ("Insert Column After", "Insert an empty column after the selected column")]
		void InsertColumnAfter (IWidgetSite context)
		{
			Gtk.Table.TableChild tc = table[(Gtk.Widget)context] as Gtk.Table.TableChild;
			AddColumn (tc.RightAttach);
		}

		[Command ("Delete Row", "Delete the selected row")]
		void DeleteRow (IWidgetSite context)
		{
			Gtk.Table.TableChild tc = table[(Gtk.Widget)context] as Gtk.Table.TableChild;
			DeleteRow (tc.TopAttach);
		}

		[Command ("Delete Column", "Delete the selected column")]
		void DeleteColumn (IWidgetSite context)
		{
			Gtk.Table.TableChild tc = table[(Gtk.Widget)context] as Gtk.Table.TableChild;
			DeleteColumn (tc.LeftAttach);
		}

		private bool hexpandable, vexpandable;
		public override bool HExpandable { get { return hexpandable; } }
		public override bool VExpandable { get { return vexpandable; } }

		protected override void SiteOccupancyChanged (WidgetSite site)
		{
			Freeze ();
			if (site.Occupied) {
				Gtk.Table.TableChild tc = table[site] as Gtk.Table.TableChild;
				tc.XOptions = 0;
				tc.YOptions = 0;
			} else
				AutoSize[site] = true;
			Thaw ();
			base.SiteOccupancyChanged (site);
		}

		protected override void SiteRemoved (WidgetSite site)
		{
			AutoSize[site] = false;
		}

#if NOT
		private const int delta = 4;

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

			Gtk.Table.TableChild tc = table[FocusChild] as Gtk.Table.TableChild;

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
#endif

		public class TableChild : Stetic.Wrapper.Container.ContainerChild {
			public static ItemGroup TableChildProperties;

			static TableChild ()
			{
				TableChildProperties = new ItemGroup ("Table Child Layout",
								      typeof (Stetic.Wrapper.Table.TableChild),
								      typeof (Gtk.Table.TableChild),
								      "TopAttach",
								      "BottomAttach",
								      "LeftAttach",
								      "RightAttach",
								      "XPadding",
								      "YPadding",
								      "AutoSize",
								      "XExpand",
								      "XFill",
								      "XShrink",
								      "YExpand",
								      "YFill",
								      "YShrink");
				TableChildProperties["XExpand"].DependsInverselyOn (TableChildProperties["AutoSize"]);
				TableChildProperties["XFill"].DependsInverselyOn (TableChildProperties["AutoSize"]);
				TableChildProperties["XFill"].DependsOn (TableChildProperties["XExpand"]);
				TableChildProperties["XShrink"].DependsInverselyOn (TableChildProperties["AutoSize"]);
				TableChildProperties["YExpand"].DependsInverselyOn (TableChildProperties["AutoSize"]);
				TableChildProperties["YFill"].DependsInverselyOn (TableChildProperties["AutoSize"]);
				TableChildProperties["YFill"].DependsOn (TableChildProperties["YExpand"]);
				TableChildProperties["YShrink"].DependsInverselyOn (TableChildProperties["AutoSize"]);
				RegisterWrapper (typeof (Stetic.Wrapper.Table.TableChild),
						 TableChildProperties);
			}

			public TableChild (IStetic stetic, Gtk.Table.TableChild tc) : base (stetic, tc) {}

			Gtk.Table.TableChild tc {
				get {
					return (Gtk.Table.TableChild)Wrapped;
				}
			}

			Stetic.Wrapper.Table parent {
				get {
					return (Stetic.Wrapper.Table)ParentWrapper;
				}
			}

			[Description ("Auto Size", "If set, the other packing properties for this cell will be automatically adjusted as other widgets are added to and removed from the table")]
			public bool AutoSize {
				get {
					return parent.AutoSize[tc.Child];
				}
				set {
					parent.AutoSize[tc.Child] = value;
					EmitNotify ("AutoSize");
				}
			}

			[Description ("Expand Horizontally", "Whether or not the table cell should expand horizontally")]
			public bool XExpand {
				get {
					return (tc.XOptions & Gtk.AttachOptions.Expand) != 0;
				}
				set {
					if (value)
						tc.XOptions |= Gtk.AttachOptions.Expand;
					else
						tc.XOptions &= ~Gtk.AttachOptions.Expand;
					EmitNotify ("XExpand");
				}
			}

			[Description ("Fill Horizontally", "Whether or not the widget should expand to fill its cell horizontally")]
			public bool XFill {
				get {
					return (tc.XOptions & Gtk.AttachOptions.Fill) != 0;
				}
				set {
					if (value)
						tc.XOptions |= Gtk.AttachOptions.Fill;
					else
						tc.XOptions &= ~Gtk.AttachOptions.Fill;
					EmitNotify ("XFill");
				}
			}

			[Description ("Shrink Horizontally", "Whether or not the table cell should shrink horizontally")]
			public bool XShrink {
				get {
					return (tc.XOptions & Gtk.AttachOptions.Shrink) != 0;
				}
				set {
					if (value)
						tc.XOptions |= Gtk.AttachOptions.Shrink;
					else
						tc.XOptions &= ~Gtk.AttachOptions.Shrink;
					EmitNotify ("XShrink");
				}
			}

			[Description ("Expand Vertically", "Whether or not the table cell should expand vertically")]
			public bool YExpand {
				get {
					return (tc.YOptions & Gtk.AttachOptions.Expand) != 0;
				}
				set {
					if (value)
						tc.YOptions |= Gtk.AttachOptions.Expand;
					else
						tc.YOptions &= ~Gtk.AttachOptions.Expand;
					EmitNotify ("YExpand");
				}
			}

			[Description ("Fill Vertically", "Whether or not the widget should expand to fill its cell vertically")]
			public bool YFill {
				get {
					return (tc.YOptions & Gtk.AttachOptions.Fill) != 0;
				}
				set {
					if (value)
						tc.YOptions |= Gtk.AttachOptions.Fill;
					else
						tc.YOptions &= ~Gtk.AttachOptions.Fill;
					EmitNotify ("YFill");
				}
			}

			[Description ("Shrink Vertically", "Whether or not the table cell should shrink vertically")]
			public bool YShrink {
				get {
					return (tc.YOptions & Gtk.AttachOptions.Shrink) != 0;
				}
				set {
					if (value)
						tc.YOptions |= Gtk.AttachOptions.Shrink;
					else
						tc.YOptions &= ~Gtk.AttachOptions.Shrink;
					EmitNotify ("YShrink");
				}
			}

			protected override void EmitNotify (string propertyName)
			{
				if (propertyName == "x-options" || propertyName == "AutoSize") {
					base.EmitNotify ("XExpand");
					base.EmitNotify ("XFill");
					base.EmitNotify ("XShrink");
				}
				if (propertyName == "y-options" || propertyName == "AutoSize") {
					base.EmitNotify ("YExpand");
					base.EmitNotify ("YFill");
					base.EmitNotify ("YShrink");
				}
				base.EmitNotify (propertyName);
				parent.Sync ();
			}
		}
	}
}
