using System;

namespace Stetic.Editor {

	public class StockItem: BaseImageCell
	{
		Gdk.Pixbuf image;
		string label;
		
		protected override void Initialize ()
		{
			base.Initialize ();
			string name = (string)Value;
			if (name != null) {
				Gtk.StockItem item = Gtk.Stock.Lookup (name);
				label = item.Label != null && item.Label.Length > 0 ? item.Label : name;
				label = label.Replace ("_", "");
				try {
					Image = Gtk.IconTheme.Default.LoadIcon (name, ImageSize, 0);
				} catch {
					Image = Gtk.IconTheme.Default.LoadIcon (Gtk.Stock.MissingImage, ImageSize, 0);;
				}
			} else {
				Image = null;
				label = "";
			}
		}
		
		protected override string GetValueText ()
		{
			return label;
		}

		protected override IPropertyEditor CreateEditor (Gdk.Rectangle cell_area, Gtk.StateType state)
		{
			return new StockItemEditor ();
		}
	}
	
	[PropertyEditor ("StockId", "Changed")]
	public class StockItemEditor : Image {

		public StockItemEditor () : base (true, false) { }
		
		public override object Value {
			get { return StockId; }
			set { StockId = (string) value; }
		}
	}
}
