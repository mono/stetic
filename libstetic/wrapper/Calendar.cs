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
			RegisterItems (typeof (Stetic.Wrapper.Calendar),
				       CalendarProperties,
				       Widget.CommonWidgetProperties);
		}

		public Calendar (IStetic stetic) : this (stetic, new Gtk.Calendar ()) {}
		public Calendar (IStetic stetic, Gtk.Calendar calendar) : base (stetic, calendar) {}
	}
}
