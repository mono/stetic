using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("Frame", "frame.png")]
	public class Frame : Gtk.Frame, Stetic.IContainerWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup FrameProperties;

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static Frame () {
			FrameProperties = new PropertyGroup ("Frame Properties",
							     typeof (Stetic.Widget.Frame),
							     "Shadow",
							     "ShadowType",
							     "Label",
							     "LabelXalign",
							     "LabelYalign",
							     "BorderWidth");

			groups = new PropertyGroup[] {
				FrameProperties,
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[0];
		}

		public Frame (IStetic stetic) : base ("Frame")
		{
			WidgetSite site = stetic.CreateWidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			Add (site);
		}

		public bool HExpandable { get { return ((WidgetSite)Child).HExpandable; } }
		public bool VExpandable { get { return ((WidgetSite)Child).VExpandable; } }

		public event ContentsChangedHandler ContentsChanged;

		private void SiteOccupancyChanged (WidgetSite site)
		{
			if (ContentsChanged != null)
				ContentsChanged (this);
		}
	}
}
