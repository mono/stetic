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
			EntryProperties = new PropertyGroup ("Entry Properties",
							     typeof (Stetic.Wrapper.Entry),
							     "Text",
							     "Editable",
							     "WidthChars",
							     "MaxLength",
							     "HasFrame",
							     "ActivatesDefault",
							     "Visibility");
			EntryExtraProperties = new PropertyGroup ("Extra Entry Properties",
								  typeof (Stetic.Wrapper.Entry),
								  "InvisibleChar",
								  "Xalign");

			groups = new PropertyGroup[] {
				EntryProperties, EntryExtraProperties,
				Widget.CommonWidgetProperties
			};
		}

		public Entry () : base () {}
	}
}
