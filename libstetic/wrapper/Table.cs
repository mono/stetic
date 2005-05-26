using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Table", "table.png", ObjectWrapperType.Container)]
	public class Table : Container {

		public static new Type WrappedType = typeof (Gtk.Table);

		internal static new void Register (Type type)
		{
			AddItemGroup (type, "Table Properties",
				      "NRows",
				      "NColumns",
				      "Homogeneous",
				      "RowSpacing",
				      "ColumnSpacing",
				      "BorderWidth");

			AddContextMenuItems (type,
					     "InsertRowBefore",
					     "InsertRowAfter",
					     "InsertColumnBefore",
					     "InsertColumnAfter",
					     "DeleteRow",
					     "DeleteColumn");
		}

		const Gtk.AttachOptions expandOpts = Gtk.AttachOptions.Expand | Gtk.AttachOptions.Fill;
		const Gtk.AttachOptions fillOpts = Gtk.AttachOptions.Fill;

		public static new Gtk.Table CreateInstance ()
		{
			return new Gtk.Table (3, 3, false);
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			Sync ();
		}

		protected override void GladeImport (string className, string id, Hashtable props)
		{
			stetic.GladeImportComplete += PostGladeSync;
			base.GladeImport (className, id, props);
		}

		public override Widget GladeImportChild (string className, string id, Hashtable props, Hashtable childprops)
		{
			string opts = (string)childprops["x_options"];
			if (opts == null)
				childprops["x_options"] = "GTK_EXPAND|GTK_FILL";
			else if (opts != "")
				childprops["x_options"] = "GTK_" + Regex.Replace (opts.ToUpper (), @"\|", "|GTK_");
			opts = (string)childprops["y_options"];
			if (opts == null)
				childprops["y_options"] = "GTK_EXPAND|GTK_FILL";
			else if (opts != "")
				childprops["y_options"] = "GTK_" + Regex.Replace (opts.ToUpper (), @"\|", "|GTK_");

			return base.GladeImportChild (className, id, props, childprops);
		}

		public override void GladeExportChild (Widget wrapper, out string className, out string internalId, out string id, out Hashtable props, out Hashtable childprops)
		{
			base.GladeExportChild (wrapper, out className, out internalId, out id, out props, out childprops);
			string opts = (string)childprops["x_options"];
			if (opts != null)
				childprops["x_options"] = Regex.Replace (opts.ToLower (), "gtk_", "");
			opts = (string)childprops["y_options"];
			if (opts != null)
				childprops["y_options"] = Regex.Replace (opts.ToLower (), "gtk_", "");
		}

		void PostGladeSync ()
		{
			Sync ();
			stetic.GladeImportComplete -= PostGladeSync;
		}

		private Gtk.Table table {
			get {
				return (Gtk.Table)Wrapped;
			}
		}

		protected override void DoSync ()
		{
			uint left, right, top, bottom;
			uint row, col;
			Gtk.Widget w;
			Gtk.Widget[,] grid;
			Gtk.Table.TableChild tc;
			Gtk.Widget[] children;
			bool addedPlaceholders = false;

			children = table.Children;
			grid = new Gtk.Widget[NRows,NColumns];

			// First fill in the placeholders in the grid. If we find any
			// placeholders covering more than one grid square, remove them.
			// (New ones will be created below.)
                        foreach (Gtk.Widget child in children) {
				if (!(child is Placeholder))
					continue;

                                tc = table[child] as Gtk.Table.TableChild;
                                left = tc.LeftAttach;
                                right = tc.RightAttach;
                                top = tc.TopAttach;
                                bottom = tc.BottomAttach;

				if (right == left + 1 && bottom == top + 1)
					grid[top,left] = child;
				else
					table.Remove (child);
			}

			// Now fill in the real widgets, knocking out any placeholders
			// they overlap. (If there are real widgets that overlap
			// placeholders, neither will be knocked out, and the layout
			// will probably end up wrong as well. But this situation
			// happens at least temporarily during glade import.)
                        foreach (Gtk.Widget child in children) {
				if (child is Placeholder)
					continue;

                                tc = table[child] as Gtk.Table.TableChild;
                                left = tc.LeftAttach;
                                right = tc.RightAttach;
                                top = tc.TopAttach;
                                bottom = tc.BottomAttach;

                                for (row = top; row < bottom; row++) {
                                        for (col = left; col < right; col++) {
						w = grid[row,col];
						if (w is Placeholder)
							table.Remove (grid[row,col]);
                                                grid[row,col] = child;
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
					w = grid[row,col];
					if (w == null) {
						w = CreatePlaceholder ();
						table.Attach (w, col, col + 1, row, row + 1);
						grid[row,col] = w;
						addedPlaceholders = true;
					} else if (!ChildVExpandable (w) || !AutoSize[w])
						allPlaceholders = false;
				}

				for (col = 0; col < NColumns; col++) {
					w = grid[row,col];
					if (!AutoSize[w])
						continue;
					tc = table[w] as Gtk.Table.TableChild;
					Gtk.AttachOptions opts = allPlaceholders ? expandOpts : fillOpts;
					if (tc.YOptions != opts)
						tc.YOptions = opts;
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
					w = grid[row,col];
					if (!ChildHExpandable (w) || !AutoSize[w]) {
						allPlaceholders = false;
						break;
					}
				}

				for (row = 0; row < NRows; row++) {
					w = grid[row,col];
					if (!AutoSize[w])
						continue;
					tc = table[w] as Gtk.Table.TableChild;
					Gtk.AttachOptions opts = allPlaceholders ? expandOpts : fillOpts;
					if (tc.XOptions != opts)
						tc.XOptions = opts;
				}

				if (allPlaceholders)
					hexpandable = true;
			}

			if (addedPlaceholders)
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
		internal void InsertRowBefore (Gtk.Widget context)
		{
			Gtk.Table.TableChild tc = table[context] as Gtk.Table.TableChild;
			AddRow (tc.TopAttach);
		}

		[Command ("Insert Row After", "Insert an empty row below the selected row")]
		internal void InsertRowAfter (Gtk.Widget context)
		{
			Gtk.Table.TableChild tc = table[context] as Gtk.Table.TableChild;
			AddRow (tc.BottomAttach);
		}

		[Command ("Insert Column Before", "Insert an empty column before the selected column")]
		internal void InsertColumnBefore (Gtk.Widget context)
		{
			Gtk.Table.TableChild tc = table[context] as Gtk.Table.TableChild;
			AddColumn (tc.LeftAttach);
		}

		[Command ("Insert Column After", "Insert an empty column after the selected column")]
		internal void InsertColumnAfter (Gtk.Widget context)
		{
			Gtk.Table.TableChild tc = table[context] as Gtk.Table.TableChild;
			AddColumn (tc.RightAttach);
		}

		[Command ("Delete Row", "Delete the selected row")]
		internal void DeleteRow (Gtk.Widget context)
		{
			Gtk.Table.TableChild tc = table[context] as Gtk.Table.TableChild;
			DeleteRow (tc.TopAttach);
		}

		[Command ("Delete Column", "Delete the selected column")]
		internal void DeleteColumn (Gtk.Widget context)
		{
			Gtk.Table.TableChild tc = table[context] as Gtk.Table.TableChild;
			DeleteColumn (tc.LeftAttach);
		}

		private bool hexpandable, vexpandable;
		public override bool HExpandable { get { return hexpandable; } }
		public override bool VExpandable { get { return vexpandable; } }

		protected override void ChildContentsChanged (Container child)
		{
			Gtk.Widget widget = child.Wrapped;
			Freeze ();
			if (AutoSize[widget]) {
				Gtk.Table.TableChild tc = table[widget] as Gtk.Table.TableChild;
				tc.XOptions = 0;
				tc.YOptions = 0;
			}
			Thaw ();

			base.ChildContentsChanged (child);
		}

		public class TableChild : Container.ContainerChild {

			public static new Type WrappedType = typeof (Gtk.Table.TableChild);

			internal static new void Register (Type type)
			{
				ItemGroup props = AddItemGroup (type, "Table Child Layout",
								"TopAttach",
								"BottomAttach",
								"LeftAttach",
								"RightAttach",
								"XPadding",
								"YPadding",
								"AutoSize",
								"XOptions/XExpand",
								"XOptions/XFill",
								"XOptions/XShrink",
								"YOptions/YExpand",
								"YOptions/YFill",
								"YOptions/YShrink");
				props["XExpand"].DependsInverselyOn (props["AutoSize"]);
				props["XFill"].DependsInverselyOn (props["AutoSize"]);
				props["XFill"].DependsOn (props["XExpand"]);
				props["XShrink"].DependsInverselyOn (props["AutoSize"]);
				props["YExpand"].DependsInverselyOn (props["AutoSize"]);
				props["YFill"].DependsInverselyOn (props["AutoSize"]);
				props["YFill"].DependsOn (props["YExpand"]);
				props["YShrink"].DependsInverselyOn (props["AutoSize"]);
			}

			Gtk.Table.TableChild tc {
				get {
					return (Gtk.Table.TableChild)Wrapped;
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
			}
		}
	}
}
