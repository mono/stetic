using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("Dialog Box", "dialog.png")]
	public class Dialog : Gtk.Dialog, Stetic.IWindowWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup DialogProperties;
		public static PropertyGroup DialogMiscProperties;

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static Dialog () {
			DialogProperties = new PropertyGroup ("Dialog Properties",
							      typeof (Stetic.Widget.Dialog),
							      "Title",
							      "Icon",
							      "WindowPosition",
							      "Modal",
							      "BorderWidth");
			DialogMiscProperties = new PropertyGroup ("Miscellaneous Dialog Properties",
								  typeof (Stetic.Widget.Dialog),
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

		public Dialog (IStetic stetic) : base ()
		{
			WidgetSite site = stetic.CreateWidgetSite (200, 200);
			site.OccupancyChanged += SiteOccupancyChanged;
			site.Show ();
			VBox.Add (site);
		}

		public bool HExpandable { get { return true; } }
		public bool VExpandable { get { return true; } }

		public event ContentsChangedHandler ContentsChanged;

		private void SiteOccupancyChanged (WidgetSite site)
		{
			if (ContentsChanged != null)
				ContentsChanged (this);
		}
	}
}
