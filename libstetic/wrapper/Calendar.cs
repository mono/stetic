using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Calendar", "calendar.png", ObjectWrapperType.Widget)]
	public class Calendar : Stetic.Wrapper.Widget {

		public static ItemGroup CalendarProperties;

		static Calendar () {
			CalendarProperties = new ItemGroup ("Calendar Properties",
							    typeof (Gtk.Calendar),
							    "ShowHeading",
							    "ShowDayNames",
							    "ShowWeekNumbers",
							    "NoMonthChange");

			groups = new ItemGroup[] {
				CalendarProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public Calendar (IStetic stetic) : this (stetic, new Gtk.Calendar ()) {}

		public Calendar (IStetic stetic, Gtk.Calendar calendar) : base (stetic, calendar) {}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }
	}
}
