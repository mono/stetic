using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Calendar", "calendar.png", ObjectWrapperType.Widget)]
	public class Calendar : Stetic.Wrapper.Widget {

		public static PropertyGroup CalendarProperties;

		static Calendar () {
			CalendarProperties = new PropertyGroup ("Calendar Properties",
								typeof (Gtk.Calendar),
								"ShowHeading",
								"ShowDayNames",
								"ShowWeekNumbers",
								"NoMonthChange");

			groups = new PropertyGroup[] {
				CalendarProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public Calendar (IStetic stetic) : this (stetic, new Gtk.Calendar ()) {}

		public Calendar (IStetic stetic, Gtk.Calendar calendar) : base (stetic, calendar) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
