using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Stock Icon", "image.png", ObjectWrapperType.Widget)]
	public class Icon : Misc {

		public static new Type WrappedType = typeof (Gtk.Image);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Stock Icon Properties",
				      "Stock",
				      "IconSize");
		}

		public static new Gtk.Image CreateInstance ()
		{
			return new Gtk.Image (Gtk.Stock.Execute, Gtk.IconSize.Dialog);
		}

		[Editor (typeof (Stetic.Editor.StockItem))]
		[Description ("Stock Icon", "The stock icon to display")]
		public string Stock {
			get {
				return ((Gtk.Image)Wrapped).Stock;
			}
			set {
				((Gtk.Image)Wrapped).Stock = value;
			}
		}

		public Gtk.IconSize IconSize {
			get {
				return (Gtk.IconSize)((Gtk.Image)Wrapped).IconSize;
			}
			set {
				((Gtk.Image)Wrapped).IconSize = (int)value;
			}
		}
	}
}
