using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class VSeparator : Gtk.VSeparator, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static VSeparator () {
			groups = new PropertyGroup[] {
				Widget.CommonWidgetProperties
			};
		}

		public VSeparator () : base () {}
	}
}
