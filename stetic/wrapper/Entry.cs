using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Entry", "entry.png")]
	public class Entry : Gtk.Entry, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup EntryProperties;
		public static PropertyGroup EntryExtraProperties;

		static Entry () {
			PropertyDescriptor[] props;

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Entry), "Text"),
				new PropertyDescriptor (typeof (Gtk.Entry), "Editable"),
				new PropertyDescriptor (typeof (Gtk.Entry), "WidthChars"),
				new PropertyDescriptor (typeof (Gtk.Entry), "MaxLength"),
				new PropertyDescriptor (typeof (Gtk.Entry), "HasFrame"),
				new PropertyDescriptor (typeof (Gtk.Entry), "ActivatesDefault"),
				new PropertyDescriptor (typeof (Gtk.Entry), "Visibility"),
			};				
			EntryProperties = new PropertyGroup ("Entry Properties", props);

			props = new PropertyDescriptor[] {
				new PropertyDescriptor (typeof (Gtk.Entry), "InvisibleChar"),
				new PropertyDescriptor (typeof (Gtk.Entry), "Xalign"),
			};
			EntryExtraProperties = new PropertyGroup ("Extra Entry Properties", props);

			groups = new PropertyGroup[] {
				EntryProperties, EntryExtraProperties,
				Widget.CommonWidgetProperties
			};
		}

		public Entry () : base () {}
	}
}
