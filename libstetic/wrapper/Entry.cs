using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Entry", "entry.png", typeof (Gtk.Entry), ObjectWrapperType.Widget)]
	public class Entry : Stetic.Wrapper.Widget {

		public static ItemGroup EntryProperties;
		public static ItemGroup EntryExtraProperties;

		static Entry () {
			EntryProperties = new ItemGroup ("Entry Properties",
							 typeof (Gtk.Entry),
							 "Text",
							 "Editable",
							 "WidthChars",
							 "MaxLength",
							 "HasFrame",
							 "ActivatesDefault",
							 "Visibility");
			EntryExtraProperties = new ItemGroup ("Extra Entry Properties",
							      typeof (Gtk.Entry),
							      "InvisibleChar",
							      "Xalign");

			RegisterWrapper (typeof (Stetic.Wrapper.Entry),
					 EntryProperties,
					 EntryExtraProperties,
					 Widget.CommonWidgetProperties);
		}

		public Entry (IStetic stetic) : this (stetic, new Gtk.Entry (), false) {}
		public Entry (IStetic stetic, Gtk.Entry entry, bool initialized) : base (stetic, entry, initialized) {}
	}
}
