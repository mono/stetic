using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Toolbar Button", "button.png", ObjectWrapperType.ToolbarItem)]
	public class ToolButton : Widget {

		public static new Type WrappedType = typeof (Gtk.ToolButton);

		internal static new void Register (Type type)
		{
			if (type == typeof (Stetic.Wrapper.ToolButton)) {
				AddItemGroup (type, "Toolbar Button Properties",
					      "Icon",
					      "Label",
					      "UseUnderline");
			}
		}

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

		protected override void GladeImport (string className, string id, Hashtable props)
		{
			string icon = GladeUtils.ExtractProperty ("icon", props);
			base.GladeImport (className, id, props);
			if (icon != null)
				Icon = "file:" + icon;
			else if (((Gtk.ToolButton)Wrapped).StockId == null)
				Icon = null;
		}

		string icon;

		[Editor (typeof (Stetic.Editor.Image))]
		[Description ("Icon", "The icon to display in the button")]
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
