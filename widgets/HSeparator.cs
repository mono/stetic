using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("HSeparator", "hseparator.png")]
	public class HSeparator : Gtk.HSeparator, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static HSeparator () {
			groups = new PropertyGroup[] {
				Widget.CommonWidgetProperties
			};
		}

		public HSeparator (IStetic stetic) : base () {}
	}
}
