using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("Calendar", "calendar.png")]
	public class Calendar : Gtk.Calendar, Stetic.IObjectWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		public static PropertyGroup CalendarProperties;

		static Calendar () {
			CalendarProperties = new PropertyGroup ("Calendar Properties",
							     typeof (Stetic.Widget.Calendar),
							     "ShowHeading",
							     "ShowDayNames",
							     "ShowWeekNumbers",
							     "NoMonthChange");

			groups = new PropertyGroup[] {
				CalendarProperties,
				Widget.CommonWidgetProperties
			};
		}

		public Calendar (IStetic stetic) {}
	}
}
