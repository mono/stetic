using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("Entry", "entry.png")]
	public class Entry : Gtk.Entry, Stetic.IWidgetWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup EntryProperties;
		public static PropertyGroup EntryExtraProperties;

		static Entry () {
			EntryProperties = new PropertyGroup ("Entry Properties",
							     typeof (Stetic.Widget.Entry),
							     "Text",
							     "Editable",
							     "WidthChars",
							     "MaxLength",
							     "HasFrame",
							     "ActivatesDefault",
							     "Visibility");
			EntryExtraProperties = new PropertyGroup ("Extra Entry Properties",
								  typeof (Stetic.Widget.Entry),
								  "InvisibleChar",
								  "Xalign");

			groups = new PropertyGroup[] {
				EntryProperties, EntryExtraProperties,
				Widget.CommonWidgetProperties
			};
		}

		public Entry (IStetic stetic) : base () {}
	}
}
