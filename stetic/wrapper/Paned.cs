using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public static class Paned {
		public static PropertyGroup PanedProperties;
		public static PropertyGroup PanedChildProperties;

		static Paned () {
			PanedProperties = new PropertyGroup ("Pane Properties",
							     typeof (Stetic.Wrapper.Paned),
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
