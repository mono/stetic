using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("VSeparator", "vseparator.png")]
	public class VSeparator : Gtk.VSeparator, Stetic.IWidgetWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static VSeparator () {
			groups = new PropertyGroup[] {
				Widget.CommonWidgetProperties
			};
		}

		public VSeparator (IStetic stetic) : base () {}
	}
}
