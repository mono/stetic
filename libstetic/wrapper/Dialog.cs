using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Dialog Box", "dialog.png", ObjectWrapperType.Window)]
	public class Dialog : Window {

		public static PropertyGroup DialogProperties;
		public static PropertyGroup DialogMiscProperties;

		static Dialog () {
			DialogProperties = new PropertyGroup ("Dialog Properties",
							      typeof (Gtk.Dialog),
							      "Title",
							      "Icon",
							      "WindowPosition",
							      "Modal",
							      "BorderWidth");
			DialogMiscProperties = new PropertyGroup ("Miscellaneous Dialog Properties",
								  typeof (Gtk.Dialog),
								  "HasSeparator",
								  "AcceptFocus",
								  "Decorated",
								  "DestroyWithParent",
								  "Gravity",
								  "Role",
								  "SkipPagerHint",
								  "SkipTaskbarHint");
			groups = new PropertyGroup[] {
				DialogProperties, DialogMiscProperties,
				Window.WindowSizeProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[0];
		}

		public Dialog (IStetic stetic) : this (stetic, new Gtk.Dialog ()) {}

		public Dialog (IStetic stetic, Gtk.Dialog dialog) : base (stetic, dialog)
		{
			dialog.VBox.Add (CreateWidgetSite (200, 200));
		}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public override PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }
	}
}
