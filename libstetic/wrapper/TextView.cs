using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Text View", "textview.png", ObjectWrapperType.Widget)]
	public class TextView : Stetic.Wrapper.Widget {

		public static PropertyGroup TextViewProperties;
		public static PropertyGroup TextViewExtraProperties;

		static TextView () {
			TextViewProperties = new PropertyGroup ("Text View Properties",
								typeof (Gtk.TextView),
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
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public TextView (IStetic stetic) : this (stetic, new Gtk.TextView ()) {}

		public TextView (IStetic stetic, Gtk.TextView textview) : base (stetic, textview) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
