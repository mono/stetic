using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Entry", "entry.png", ObjectWrapperType.Widget)]
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

		public Entry (IStetic stetic) : this (stetic, new Gtk.Entry ()) {}
		public Entry (IStetic stetic, Gtk.Entry entry) : base (stetic, entry) {}
	}
}
