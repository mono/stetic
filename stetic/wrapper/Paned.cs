using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public static class Paned {
		public static PropertyGroup PanedProperties;
		public static PropertyGroup PanedChildProperties;

		static Paned () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Paned), "MinPosition"),
				new PropertyDescriptor (typeof (Gtk.Paned), "MaxPosition"),
				new PropertyDescriptor (typeof (Gtk.Container), "BorderWidth"),
			};
			PanedProperties = new PropertyGroup ("Pane Properties", props);

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Paned.PanedChild), "Resize"),
				new PropertyDescriptor (typeof (Gtk.Paned.PanedChild), "Shrink"),
			};
			PanedChildProperties = new PropertyGroup ("Pane Child Layout", props);
		}
	}
}
