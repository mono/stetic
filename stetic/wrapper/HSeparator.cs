using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class HSeparator : Gtk.HSeparator, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static HSeparator () {
			groups = new PropertyGroup[] {
				Widget.CommonWidgetProperties
			};
		}

		public HSeparator () : base () {}
	}
}
