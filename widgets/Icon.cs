using Gtk;
using System;
using System.Collections;
using System.ComponentModel;

namespace Stetic.Widget {

	[WidgetWrapper ("Stock Icon", "image.png")]
	public class Icon : Gtk.Image, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup IconProperties;

		static Icon () {
			IconProperties = new PropertyGroup ("Stock Icon Properties",
							    typeof (Stetic.Widget.Icon),
							    "Stock",
							    "IconSize");

			groups = new PropertyGroup[] {
				IconProperties, Misc.MiscProperties,
				Widget.CommonWidgetProperties
			};
		}

		public Icon (IStetic stetic) : base (Gtk.Stock.Execute, Gtk.IconSize.Dialog) {}

		[Editor (typeof (Stetic.Editor.StockItem), typeof (Gtk.Widget))]
		public new string Stock {
			get {
				return base.Stock;
			}
			set {
				base.Stock = value;
			}
		}

		public new Gtk.IconSize IconSize {
			get {
				return (Gtk.IconSize)base.IconSize;
			}
			set {
				base.IconSize = (int)value;
			}
		}
	}
}
