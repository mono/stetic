using Gtk;
using System;
using System.Collections;

namespace Stetic.Widget {

	[WidgetWrapper ("Expander", "expander.png")]
	public class Expander : Gtk.Expander, Stetic.IContainerWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup ExpanderProperties;

		static PropertyGroup[] childgroups;
		public PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }

		static Expander () {
			ExpanderProperties = new PropertyGroup ("Expander Properties",
								typeof (Stetic.Widget.Expander),
								"Expanded",
								"Label",
								"UseMarkup",
								"UseUnderline",
								"Spacing",
								"BorderWidth");
			groups = new PropertyGroup[] {
				ExpanderProperties,
				Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[0];
		}

		WidgetSite site;

		public Expander (IStetic stetic) : base ("Expander")
		{
			site = stetic.CreateWidgetSite ();
			site.OccupancyChanged += SiteOccupancyChanged;
			Add (site);
		}

		public bool HExpandable { get { return Expanded && site.HExpandable; } }
		public bool VExpandable { get { return Expanded && site.VExpandable; } }

		public event ContentsChangedHandler ContentsChanged;

		private void SiteOccupancyChanged (WidgetSite site)
		{
			if (ContentsChanged != null)
				ContentsChanged (this);
		}

		protected override void OnActivated ()
		{
			base.OnActivated ();
			if (ContentsChanged != null)
				ContentsChanged (this);
		}
	}
}
