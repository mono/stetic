using Gtk;
using System;

namespace Stetic.Widget {

	public static class Paned {
		public static PropertyGroup PanedProperties;
		public static PropertyGroup PanedChildProperties;

		static Paned () {
			PanedProperties = new PropertyGroup ("Pane Properties",
							     typeof (Gtk.Paned),
							     "MinPosition",
							     "MaxPosition",
							     "BorderWidth");

			PanedChildProperties = new PropertyGroup ("Pane Child Layout",
								  typeof (Gtk.Paned.PanedChild),
								  "Resize",
								  "Shrink");
		}
	}
}
