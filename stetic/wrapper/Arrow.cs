using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class Arrow : Gtk.Arrow, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup ArrowProperties;
		public static PropertyGroup ArrowExtraProperties;

		static Arrow () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Arrow), "ArrowType"),
				new PropertyDescriptor (typeof (Gtk.Arrow), "ShadowType"),
			};				
			ArrowProperties = new PropertyGroup ("Arrow Properties", props);

			groups = new PropertyGroup[] {
				ArrowProperties, Widget.CommonWidgetProperties
			};
		}

		public Arrow () : base (ArrowType.Left, ShadowType.None) {}
	}
}
