using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Table", "table.png", ObjectWrapperType.Container)]
	public class Table : Stetic.Wrapper.Container {

		public static ItemGroup TableProperties;
		public static ItemGroup TableChildProperties;

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

			TableChildProperties = new ItemGroup ("Table Child Layout",
							      typeof (Gtk.Table.TableChild),
							      "TopAttach",
							      "BottomAttach",
							      "LeftAttach",
							      "RightAttach",
							      "XPadding",
							      "YPadding",
//							      "AutoSize",
							      "XOptions",
							      "YOptions");
//			TableChildProperties["XOptions"].DependsOn (TableChildProperties["AutoSize"]);
//			TableChildProperties["YOptions"].DependsOn (TableChildProperties["AutoSize"]);

			RegisterChildItems (typeof (Stetic.Wrapper.Table),
					    TableChildProperties);

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
			table.Removed += SiteRemoved;
			Sync ();
		}

		private Gtk.Table table {
			get {
				return (Gtk.Table)Wrapped;
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
			Gtk.Table.TableChild tc;
			Gtk.Widget[] children;

			if (freeze > 0)
				return;
			freeze = 1;

			children = table.Children;

			grid = new WidgetSite[NRows,NColumns];

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
						site.ChildNotified += ChildNotification;
						site.Show ();
						table.Attach (site, col, col + 1, row, row + 1);
						grid[row,col] = site;
					} else if (!grid[row,col].VExpandable)
						allPlaceholders = false;
				}

				for (col = 0; col < NColumns; col++) {
					site = grid[row,col];
					tc = table[site] as Gtk.Table.TableChild;
//					if (!tc.AutoSize)
//						continue;
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
					tc = table[site] as Gtk.Table.TableChild;
//					if (!tc.AutoSize)
//						continue;
					tc.XOptions = allPlaceholders ? expandOpts : fillOpts;
				}

				if (allPlaceholders)
					hexpandable = true;
			}

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

		[Command ("Insert Row Before")]
		void InsertRowBefore (IWidgetSite context)
		{
			Gtk.Table.TableChild tc = table[(Gtk.Widget)context] as Gtk.Table.TableChild;
			AddRow (tc.TopAttach);
		}

		[Command ("Insert Row After")]
		void InsertRowAfter (IWidgetSite context)
		{
			Gtk.Table.TableChild tc = table[(Gtk.Widget)context] as Gtk.Table.TableChild;
			AddRow (tc.BottomAttach);
		}

		[Command ("Insert Column Before")]
		void InsertColumnBefore (IWidgetSite context)
		{
			Gtk.Table.TableChild tc = table[(Gtk.Widget)context] as Gtk.Table.TableChild;
			AddColumn (tc.LeftAttach);
		}

		[Command ("Insert Column After")]
		void InsertColumnAfter (IWidgetSite context)
		{
			Gtk.Table.TableChild tc = table[(Gtk.Widget)context] as Gtk.Table.TableChild;
			AddColumn (tc.RightAttach);
		}

		[Command ("Delete Row")]
		void DeleteRow (IWidgetSite context)
		{
			Gtk.Table.TableChild tc = table[(Gtk.Widget)context] as Gtk.Table.TableChild;
			DeleteRow (tc.TopAttach);
		}

		[Command ("Delete Column")]
		void DeleteColumn (IWidgetSite context)
		{
			Gtk.Table.TableChild tc = table[(Gtk.Widget)context] as Gtk.Table.TableChild;
			DeleteColumn (tc.LeftAttach);
		}

		private bool hexpandable, vexpandable;
		public override bool HExpandable { get { return hexpandable; } }
		public override bool VExpandable { get { return vexpandable; } }

		void SiteRemoved (object obj, Gtk.RemovedArgs args)
		{
			WidgetSite site = args.Widget as WidgetSite;

			if (site != null)
				site.ChildNotified -= ChildNotification;
		}

		protected override void SiteOccupancyChanged (WidgetSite site)
		{
			Freeze ();
			if (site.Occupied) {
				Gtk.Table.TableChild tc = table[site] as Gtk.Table.TableChild;
				tc.XOptions = 0;
				tc.YOptions = 0;
			}
			Thaw ();
			base.SiteOccupancyChanged (site);
		}

		private void ChildNotification (object o, Gtk.ChildNotifiedArgs args)
		{
//			if (!(args.Pspec is ParamSpecUInt))
//				return;

			Sync ();
		}

		private const int delta = 4;

#if NOT
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
	}
}
