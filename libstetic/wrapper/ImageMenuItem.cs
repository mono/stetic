using System;
using System.Collections;

namespace Stetic.Wrapper {

	public class ImageMenuItem : MenuItem {

		public static new Gtk.ImageMenuItem CreateInstance ()
		{
			// Use the ctor that will create an AccelLabel
			return new Gtk.ImageMenuItem ("");
		}

		protected override void GladeImport (string className, string id, Hashtable props)
		{
			Gtk.StockItem stockItem = Gtk.StockItem.Zero;
			string use_stock = GladeUtils.ExtractProperty ("use_stock", props);
			if (use_stock == "True") {
				stockItem = Gtk.Stock.Lookup (props["label"] as string);
				if (stockItem.Label != null)
					props.Remove ("label");
			}
			base.GladeImport (className, id, props);

			if (stockItem.StockId != null)
				Image = "stock:" + stockItem.StockId;
			if (stockItem.Keyval != 0)
				Accelerator = Gtk.Accelerator.Name (stockItem.Keyval, stockItem.Modifier);
		}

		string image;

		public string Image {
			get {
				return image;
			}
			set {
				image = value;

				Gtk.Widget icon;
				Gtk.StockItem stockItem = Gtk.StockItem.Zero;

				if (image.StartsWith ("stock:"))
					stockItem = Gtk.Stock.Lookup (image.Substring (6));

				if (stockItem.StockId != null) {
					icon = new Gtk.Image (stockItem.StockId, Gtk.IconSize.Menu);
					Label = stockItem.Label;
					UseUnderline = true;
				} else if (image.StartsWith ("file:"))
					icon = new Gtk.Image (image.Substring (5));
				else
					icon = new Gtk.Image (Gtk.Stock.MissingImage, Gtk.IconSize.Menu);

				((Gtk.ImageMenuItem)Wrapped).Image = icon;
			}
		}
	}
}
