using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("Arrow", "arrow.png")]
	public class Arrow : Gtk.Arrow, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup ArrowProperties;

		static Arrow () {
			ArrowProperties = new PropertyGroup ("Arrow Properties",
							     typeof (Gtk.Arrow),
							     "ArrowType",
							     "ShadowType");

			groups = new PropertyGroup[] {
				ArrowProperties, Misc.MiscProperties,
				Widget.CommonWidgetProperties
			};
		}

		public Arrow (IStetic stetic) : base (ArrowType.Left, ShadowType.None) {}
	}
}
