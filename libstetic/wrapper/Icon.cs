using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Stock Icon", "image.png", typeof (Gtk.Image), ObjectWrapperType.Widget)]
	public class Icon : Misc {

		public static ItemGroup IconProperties;

		static Icon () {
			IconProperties = new ItemGroup ("Stock Icon Properties",
							typeof (Stetic.Wrapper.Icon),
							typeof (Gtk.Image),
							"Stock",
							"IconSize");
			RegisterWrapper (typeof (Stetic.Wrapper.Icon),
					 IconProperties,
					 Misc.MiscProperties,
					 Widget.CommonWidgetProperties);
		}

		public Icon (IStetic stetic) : this (stetic, new Gtk.Image (Gtk.Stock.Execute, Gtk.IconSize.Dialog), false) {}
		public Icon (IStetic stetic, Gtk.Image icon, bool initialized) : base (stetic, icon, initialized) {}

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
