using System;
using System.Collections;

namespace Stetic.Editor {

	[PropertyEditor ("StockId", "Changed")]
	public class StockItem : Gtk.ComboBox, IComparer {

		const int IconColumn = 0;
		const int LabelColumn = 1;

		ArrayList stockIds;

		public StockItem ()
		{
			Gtk.ListStore store = new Gtk.ListStore (typeof (string), typeof (string));

			ArrayList stockItems = new ArrayList ();
			foreach (string id in Gtk.Stock.ListIds ()) {
				Gtk.StockItem item = Gtk.Stock.Lookup (id);
				if (item.StockId == null)
					continue;
				stockItems.Add (item);
			}
			stockItems.Sort (this);

			stockIds = new ArrayList ();
			foreach (Gtk.StockItem item in stockItems) {
				string markup;

				stockIds.Add (item.StockId);

				int uline = item.Label.IndexOf ('_');
				if (uline != -1 && uline < item.Label.Length - 2)
					markup = item.Label.Substring (0, uline) + "<u>" + item.Label.Substring (uline + 1, 1) + "</u>" + item.Label.Substring (uline + 2);
				else
					markup = item.Label;
				store.AppendValues (item.StockId, markup);
			}

			Model = store;

			Gtk.CellRendererPixbuf iconRenderer = new Gtk.CellRendererPixbuf ();
			iconRenderer.StockSize = (uint)Gtk.IconSize.Menu;
			PackStart (iconRenderer, false);
			AddAttribute (iconRenderer, "stock-id", IconColumn);

			Gtk.CellRendererText labelRenderer = new Gtk.CellRendererText ();
			PackStart (labelRenderer, true);
			AddAttribute (labelRenderer, "markup", LabelColumn);
		}

		public int Compare (object itemx, object itemy)
		{
			Gtk.StockItem x = (Gtk.StockItem)itemx;
			Gtk.StockItem y = (Gtk.StockItem)itemy;

			return string.Compare (x.Label.Replace ("_", ""),
					       y.Label.Replace ("_", ""));
		}

		public string StockId {
			get {
				return (string)stockIds[Active];
			}
			set {
				int id = stockIds.IndexOf (value);
				if (id != -1)
					Active = id;
			}
		}
	}
}
