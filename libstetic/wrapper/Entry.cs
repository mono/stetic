using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Entry", "entry.png", ObjectWrapperType.Widget)]
	public class Entry : Stetic.Wrapper.Widget {

		public static PropertyGroup EntryProperties;
		public static PropertyGroup EntryExtraProperties;

		static Entry () {
			EntryProperties = new PropertyGroup ("Entry Properties",
							     typeof (Gtk.Entry),
							     "Text",
							     "Editable",
							     "WidthChars",
							     "MaxLength",
							     "HasFrame",
							     "ActivatesDefault",
							     "Visibility");
			EntryExtraProperties = new PropertyGroup ("Extra Entry Properties",
								  typeof (Gtk.Entry),
								  "InvisibleChar",
								  "Xalign");

			groups = new PropertyGroup[] {
				EntryProperties, EntryExtraProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public Entry (IStetic stetic) : this (stetic, new Gtk.Entry ()) {}

		public Entry (IStetic stetic, Gtk.Entry entry) : base (stetic, entry) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
