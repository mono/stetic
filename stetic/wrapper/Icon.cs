using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;
using System.ComponentModel;

namespace Stetic.Wrapper {

	public class Icon : Gtk.Image, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup IconProperties;

		static Icon () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Stetic.Wrapper.Icon), "StockId"),
				new PropertyDescriptor (typeof (Stetic.Wrapper.Icon), "Size"),
			};				
			IconProperties = new PropertyGroup ("Stock Icon Properties", props);

			groups = new PropertyGroup[] {
				IconProperties, Misc.MiscProperties,
				Widget.CommonWidgetProperties
			};
		}

		public Icon () : base (Gtk.Stock.Execute, Gtk.IconSize.Dialog) {}

		[Editor (typeof (Stetic.Editor.StockItem), typeof (Gtk.Widget))]
		public string StockId {
			get {
				return Stock;
			}
			set {
				Stock = value;
			}
		}

		public Gtk.IconSize Size {
			get {
				return (Gtk.IconSize)IconSize;
			}
			set {
				IconSize = (int)value;
			}
		}
	}
}
