using System;
using System.Collections;

namespace Stetic.Editor {

	[PropertyEditor ("StockId", "Changed")]
	public class StockItem : Gtk.ComboBox {

		const int IconColumn = 0;
		const int LabelColumn = 1;

		ArrayList stockIds;

		public StockItem ()
		{
			Gtk.ListStore store = new Gtk.ListStore (typeof (string), typeof (string));

			stockIds = new ArrayList ();

			string[] allIds = Gtk.Stock.ListIds ();
			Array.Sort (allIds);

			foreach (string id in allIds) {
				Gtk.StockItem item;
				string markup;

				item = Gtk.Stock.Lookup (id);
				if (item.StockId == null)
					continue;
				stockIds.Add (id);

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
