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
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.TextView), "Editable"),
				new PropertyDescriptor (typeof (Gtk.TextView), "CursorVisible"),
				new PropertyDescriptor (typeof (Gtk.TextView), "Overwrite"),
				new PropertyDescriptor (typeof (Gtk.TextView), "AcceptsTab"),
				new PropertyDescriptor (typeof (Gtk.TextView), "Tabs"),
				new PropertyDescriptor (typeof (Gtk.TextView), "Buffer.Text"),
				new PropertyDescriptor (typeof (Gtk.TextView), "Justification"),
				new PropertyDescriptor (typeof (Gtk.TextView), "WrapMode"),
				new PropertyDescriptor (typeof (Gtk.TextView), "PixelsAboveLines"),
				new PropertyDescriptor (typeof (Gtk.TextView), "PixelsBelowLines"),
				new PropertyDescriptor (typeof (Gtk.TextView), "PixelsInsideWrap"),
				new PropertyDescriptor (typeof (Gtk.TextView), "RightMargin"),
				new PropertyDescriptor (typeof (Gtk.TextView), "LeftMargin"),
				new PropertyDescriptor (typeof (Gtk.TextView), "Indent"),
			};				
			TextViewProperties = new PropertyGroup ("Text View Properties", props);

			groups = new PropertyGroup[] {
				TextViewProperties,
				Widget.CommonWidgetProperties
			};
		}

		public TextView () : base () {}
	}
}
