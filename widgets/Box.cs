using Gtk;
using System;

namespace Stetic.Widget {

	public static class Box {
		public static PropertyGroup BoxProperties;
		public static PropertyGroup BoxChildProperties;

		static Box () {
			BoxProperties = new PropertyGroup ("Box Properties",
							   typeof (Gtk.Box),
							   "Homogeneous",
							   "Spacing",
							   "BorderWidth");
			BoxChildProperties = new PropertyGroup ("Box Child Layout",
								typeof (Gtk.Box.BoxChild),
								"PackType",
								"Position",
								"Expand",
								"Fill",
								"Padding");
		}
	}
}
