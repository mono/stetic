using System;
using System.Collections;
using System.ComponentModel;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Stock Icon", "image.png", ObjectWrapperType.Widget)]
	public class Icon : Misc {

		public static ItemGroup IconProperties;

		static Icon () {
			IconProperties = new ItemGroup ("Stock Icon Properties",
							typeof (Stetic.Wrapper.Icon),
							typeof (Gtk.Image),
							"Stock",
							"IconSize");

			groups = new ItemGroup[] {
				IconProperties, Misc.MiscProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public Icon (IStetic stetic) : this (stetic, new Gtk.Image (Gtk.Stock.Execute, Gtk.IconSize.Dialog)) {}

		public Icon (IStetic stetic, Gtk.Image icon) : base (stetic, icon) {}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }

		[Editor (typeof (Stetic.Editor.StockItem), typeof (Gtk.Widget))]
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
