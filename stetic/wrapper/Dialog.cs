using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	[WidgetWrapper ("Dialog Box", "dialog.png", WidgetType.Window)]
	public class Dialog : Gtk.Dialog, Stetic.IContainerWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup DialogProperties;
		public static PropertyGroup DialogMiscProperties;

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static Dialog () {
			DialogProperties = new PropertyGroup ("Dialog Properties",
							      typeof (Stetic.Wrapper.Dialog),
							      "Title",
							      "Icon",
							      "Type",
							      "TypeHint",
							      "WindowPosition",
							      "Modal",
							      "BorderWidth");
			DialogMiscProperties = new PropertyGroup ("Miscellaneous Dialog Properties",
								  typeof (Stetic.Wrapper.Dialog),
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
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[0];
		}

		public Dialog () : base ()
		{
			WidgetSite site = new WidgetSite (200, 200);
			site.Show ();
			VBox.Add (site);
		}

		public bool HExpandable { get { return true; } }
		public bool VExpandable { get { return true; } }

		public event ExpandabilityChangedHandler ExpandabilityChanged;
	}
}
