using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Text View", "textview.png")]
	public class TextView : Gtk.TextView, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup TextViewProperties;
		public static PropertyGroup TextViewExtraProperties;

		static TextView () {
			TextViewProperties = new PropertyGroup ("Text View Properties",
								typeof (Stetic.Wrapper.TextView),
								"Editable",
								"CursorVisible",
								"Overwrite",
								"AcceptsTab",
								"Tabs",
								"Buffer.Text",
								"Justification",
								"WrapMode",
								"PixelsAboveLines",
								"PixelsBelowLines",
								"PixelsInsideWrap",
								"RightMargin",
								"LeftMargin",
								"Indent");

			groups = new PropertyGroup[] {
				TextViewProperties,
				Widget.CommonWidgetProperties
			};
		}

		public TextView () : base () {}
	}
}
