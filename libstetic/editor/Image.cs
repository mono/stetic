using System;
using System.Collections;

namespace Stetic.Editor {

	public class Image : Gtk.HBox {
		Gtk.Image image;
		Gtk.ComboBoxEntry combo;
		Gtk.Entry entry;
		Gtk.Button button;
		Gtk.ListStore store;

		const int IconColumn = 0;
		const int LabelColumn = 1;

		static string[] stockIds, stockLabels;
		static int imgWidth, imgHeight;

		static Image ()
		{
			ArrayList items = new ArrayList (), nonItems = new ArrayList ();
			foreach (string id in Gtk.Stock.ListIds ()) {
				Gtk.StockItem item = Gtk.Stock.Lookup (id);
				if (item.StockId == null)
					nonItems.Add (id);
				else {
					item.Label = item.Label.Replace ("_", "");
					items.Add (item);
				}
			}
			items.Sort (new StockItemSorter ());
			nonItems.Sort ();

			stockIds = new string[items.Count + nonItems.Count];
			stockLabels = new string[items.Count + nonItems.Count];
			for (int i = 0; i < items.Count; i++) {
				stockIds[i] = ((Gtk.StockItem)items[i]).StockId;
				stockLabels[i] = ((Gtk.StockItem)items[i]).Label;
			}
			for (int i = 0; i < nonItems.Count; i++) {
				stockIds[i + items.Count] = nonItems[i] as string;
				stockLabels[i + items.Count] = nonItems[i] as string;
			}

			Gtk.Icon.SizeLookup (Gtk.IconSize.Button, out imgWidth, out imgHeight);
		}

		class StockItemSorter : IComparer {
			public int Compare (object itemx, object itemy)
			{
				Gtk.StockItem x = (Gtk.StockItem)itemx;
				Gtk.StockItem y = (Gtk.StockItem)itemy;

				return string.Compare (x.Label, y.Label);
			}
		}

		public Image () : this (true, true) {}

		public Image (bool allowStock, bool allowFile) : base (false, 6)
		{
			image = new Gtk.Image (Gnome.Stock.Blank, Gtk.IconSize.Button);
			PackStart (image, false, false, 0);

			if (allowStock) {
				store = new Gtk.ListStore (typeof (string), typeof (string));
				store.AppendValues (Gnome.Stock.Blank, "(None)");
				for (int i = 0; i < stockIds.Length; i++)
					store.AppendValues (stockIds[i], stockLabels[i]);

				combo = new Gtk.ComboBoxEntry (store, LabelColumn);
				Gtk.CellRendererPixbuf iconRenderer = new Gtk.CellRendererPixbuf ();
				iconRenderer.StockSize = (uint)Gtk.IconSize.Menu;
				combo.PackStart (iconRenderer, false);
				combo.Reorder (iconRenderer, 0);
				combo.AddAttribute (iconRenderer, "stock-id", IconColumn);
				PackStart (combo, true, true, 0);
				combo.Changed += combo_Changed;

				entry = (Gtk.Entry)combo.Child;
				entry.Changed += entry_Changed;

				useStock = true;
			}

			if (allowFile) {
				if (!allowStock) {
					entry = new Gtk.Entry ();
					PackStart (entry, true, true, 0);
					entry.Changed += entry_Changed;
				}

				button = new Gtk.Button ();
				Gtk.Image icon = new Gtk.Image (Gtk.Stock.Open, Gtk.IconSize.Button);
				button.Add (icon);
				PackStart (button, false, false, 0);
				button.Clicked += button_Clicked;
			}
		}

		public event EventHandler Changed;

		bool syncing;

		void combo_Changed (object obj, EventArgs args)
		{
			if (syncing)
				return;

			useStock = true;
			syncing = true;
			if (combo.Active > 0)
				StockId = stockIds[combo.Active - 1];
			else
				StockId = null;
			if (Changed != null)
				Changed (this, EventArgs.Empty);
			syncing = false;
		}

		void entry_Changed (object obj, EventArgs args)
		{
			if (syncing)
				return;

			useStock = true;
			syncing = true;
			StockId = entry.Text;
			if (Changed != null)
				Changed (this, EventArgs.Empty);
			syncing = false;
		}

		void button_Clicked (object obj, EventArgs args)
		{
			Gtk.FileChooserDialog dialog =
				new Gtk.FileChooserDialog ("Image", null, Gtk.FileChooserAction.Open,
							   Gtk.Stock.Cancel, Gtk.ResponseType.Cancel,
							   Gtk.Stock.Open, Gtk.ResponseType.Ok);
			int response = dialog.Run ();
			string file = dialog.Filename;
			dialog.Destroy ();

			if (response == (int)Gtk.ResponseType.Ok) {
				syncing = true;
				useStock = false;
				entry.Text = file;
				File = file;
				if (Changed != null)
					Changed (this, EventArgs.Empty);
				syncing = false;
			}
		}

		bool useStock;

		string stockId;
		public string StockId {
			get {
				return stockId;
			}
			set {
				useStock = true;
				stockId = value;
				file = null;

				image.SetFromStock (stockId, Gtk.IconSize.Button);

				if (!syncing) {
					int id = Array.IndexOf (stockIds, value);
					if (id != -1) {
						syncing = true;
						combo.Active = id + 1;
						syncing = false;
					}
				}
			}
		}

		string file;
		public string File {
			get {
				return file;
			}
			set {
				useStock = false;
				stockId = null;
				file = value;

				if (value == null)
					value = "";

				try {
					image.Pixbuf = new Gdk.Pixbuf (value, imgWidth, imgHeight);
				} catch {
					image.SetFromStock (Gnome.Stock.Blank, Gtk.IconSize.Button);
				}

				if (!syncing) {
					syncing = true;
					entry.Text = value;
					syncing = false;
				}
			}
		}

		public string Value {
			get {
				if (useStock)
					return "stock:" + StockId;
				else
					return "file:" + File;
			}
			set {
				if (value.StartsWith ("stock:"))
					StockId = value.Substring (6);
				else if (value.StartsWith ("file:"))
					File = value.Substring (5);
			}
		}
	}
}
