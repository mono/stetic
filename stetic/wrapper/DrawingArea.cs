using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class DrawingArea : Gtk.DrawingArea, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static DrawingArea () {
			groups = new PropertyGroup[] {
				Widget.CommonWidgetProperties
			};
		}

		public DrawingArea () : base () {}
	}
}
