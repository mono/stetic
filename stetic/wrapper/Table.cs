using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class Table : Gtk.Table, Stetic.IObjectWrapper, Stetic.IDesignTimeContainer {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup TableProperties;

		static Table () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Table), "NRows"),
				new PropertyDescriptor (typeof (Gtk.Table), "NColumns"),
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
		}

		const AttachOptions expandOpts = AttachOptions.Expand | AttachOptions.Fill;
		const AttachOptions fillOpts = AttachOptions.Fill;

		public Table (uint rows, uint cols, bool homogeneous) : base (rows, cols, homogeneous)
		{
			Sync ();
		}

		private bool syncing;
		private void Sync ()
		{
			uint left, right, top, bottom;
			uint row, col;
			WidgetSite site;
			WidgetSite[,] grid;
			Table.TableChild tc;
			Gtk.Widget[] children;

			if (syncing)
				return;
			syncing = true;

			children = Children;

			grid = new WidgetSite[NRows,NColumns];

			// First fill in the placeholders in the grid. If we find any
			// placeholders covering more than one grid square, remove them.
			// (New ones will be created below.)
                        foreach (Gtk.Widget child in children) {
				site = (WidgetSite) child;
				if (site.Occupied)
					continue;

                                tc = this[child] as Table.TableChild;
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

                                tc = this[child] as Table.TableChild;
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
						site.OccupancyChanged += ChildOccupancyChanged;
						site.ChildNotified += ChildNotification;
						site.Show ();
						Attach (site, col, col + 1, row, row + 1);
						grid[row,col] = site;
					} else if (!grid[row,col].VExpandable)
						allPlaceholders = false;
				}

				for (col = 0; col < NColumns; col++) {
					site = grid[row,col];
					if (site.Occupied)
						continue;
					tc = this[site] as Table.TableChild;
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
					if (site.Occupied)
						continue;
					tc = this[site] as Table.TableChild;
					tc.XOptions = allPlaceholders ? expandOpts : fillOpts;
				}

				if (allPlaceholders)
					hexpandable = true;
			}

			syncing = false;

			if (OccupancyChanged != null)
				OccupancyChanged (this);
		}

		private bool hexpandable, vexpandable;
		public bool HExpandable { get { return hexpandable; } }
		public bool VExpandable { get { return vexpandable; } }

		public event OccupancyChangedHandler OccupancyChanged;

		protected override void OnRemoved (Gtk.Widget w)
		{
			WidgetSite site = w as WidgetSite;

			if (site == null)
				return;

			site.OccupancyChanged -= ChildOccupancyChanged;
			site.ChildNotified -= ChildNotification;

			base.OnRemoved (w);
		}

		private void ChildOccupancyChanged (IDesignTimeContainer isite)
		{
			WidgetSite site = (WidgetSite)isite;

			if (site.Occupied) {
				Table.TableChild tc = this[site] as Table.TableChild;
				tc.XOptions = 0;
				tc.YOptions = 0;
			}
			Sync ();
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

		private void AddColumn (uint col)
		{
			Resize (NRows, NColumns + 1);
			foreach (Gtk.Widget w in Children) {
				Table.TableChild tc = this[w] as Table.TableChild;

				if (tc.RightAttach > col)
					tc.RightAttach++;
				if (tc.LeftAttach >= col)
					tc.LeftAttach++;
			}
			Sync ();
		}

		private void AddRow (uint row)
		{
			Resize (NRows + 1, NColumns);
			foreach (Gtk.Widget w in Children) {
				Table.TableChild tc = this[w] as Table.TableChild;

				if (tc.BottomAttach > row)
					tc.BottomAttach++;
				if (tc.TopAttach >= row)
					tc.TopAttach++;
			}
			Sync ();
		}
	}
}
