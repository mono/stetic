using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Calendar", "calendar.png", typeof (Gtk.Calendar), ObjectWrapperType.Widget)]
	public class Calendar : Stetic.Wrapper.Widget {

		public static ItemGroup CalendarProperties;

		static Calendar () {
			CalendarProperties = new ItemGroup ("Calendar Properties",
							    typeof (Gtk.Calendar),
							    "ShowHeading",
							    "ShowDayNames",
							    "ShowWeekNumbers",
							    "NoMonthChange");
			RegisterWrapper (typeof (Stetic.Wrapper.Calendar),
					 CalendarProperties,
					 Widget.CommonWidgetProperties);
		}

		public Calendar (IStetic stetic) : this (stetic, new Gtk.Calendar (), false) {}

		
		public Calendar (IStetic stetic, Gtk.Calendar calendar, bool initialized) : base (stetic, calendar, initialized) {}
	}
}
