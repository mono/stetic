using System;
using System.Xml;

namespace Stetic.Wrapper {

	public class ToolButton : Widget {

		public static new Gtk.ToolButton CreateInstance ()
		{
			return new Gtk.ToolButton (Gtk.Stock.New);
		}

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			Gtk.ToolButton toolbutton = (Gtk.ToolButton)Wrapped;

			if (toolbutton.StockId != null)
				icon = "stock:" + toolbutton.StockId;
		}

		public override void GladeImport (XmlElement elem)
		{
			string icon = (string)GladeUtils.ExtractProperty (elem, "icon", "");
			base.GladeImport (elem);
			if (icon != "")
				Icon = "file:" + icon;
			else if (((Gtk.ToolButton)Wrapped).StockId == null)
				Icon = null;
		}

		string icon;
		public string Icon {
			get {
				return icon;
			}
			set {
				Gtk.ToolButton toolbutton = (Gtk.ToolButton)Wrapped;

				icon = value;
				if (icon != null && icon.StartsWith ("stock:")) {
					toolbutton.IconWidget = null;
					Gtk.StockItem item = Gtk.Stock.Lookup (icon.Substring (6));
					if (item.Label != null) {
						toolbutton.StockId = item.StockId;
						toolbutton.Label = item.Label;
						toolbutton.UseUnderline = true;
					}
				} else if (icon != null && icon.StartsWith ("file:")) {
					toolbutton.IconWidget = new Gtk.Image (icon.Substring (5));
					toolbutton.IconWidget.Show ();
				} else {
					toolbutton.IconWidget = new Gtk.Image (Gtk.Stock.MissingImage, Gtk.IconSize.SmallToolbar);
					toolbutton.IconWidget.Show ();
				}
			}
		}
	}
}
