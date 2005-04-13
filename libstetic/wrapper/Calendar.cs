using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Calendar", "calendar.png", ObjectWrapperType.Widget)]
	public class Calendar : Widget {

		public static new Type WrappedType = typeof (Gtk.Calendar);

		internal static new void Register (Type type)
		{
			AddItemGroup (type, "Calendar Properties",
				      "ShowHeading",
				      "ShowDayNames",
				      "ShowWeekNumbers",
				      "NoMonthChange");
		}

		protected override void GladeImport (string className, string id, Hashtable props)
		{
			string display_options = GladeUtils.ExtractProperty ("display_options", props);
			base.GladeImport (className, id, props);

			if (display_options != null) {
				Gtk.CalendarDisplayOptions options = 0;

				if (display_options.IndexOf ("SHOW_HEADING") != -1)
					options |= Gtk.CalendarDisplayOptions.ShowHeading;
				if (display_options.IndexOf ("SHOW_DAY_NAMES") != -1)
					options |= Gtk.CalendarDisplayOptions.ShowDayNames;
				if (display_options.IndexOf ("NO_MONTH_CHANGE") != -1)
					options |= Gtk.CalendarDisplayOptions.NoMonthChange;
				if (display_options.IndexOf ("SHOW_WEEK_NUMBERS") != -1)
					options |= Gtk.CalendarDisplayOptions.ShowWeekNumbers;
				if (display_options.IndexOf ("WEEK_START_MONDAY") != -1)
					options |= Gtk.CalendarDisplayOptions.WeekStartMonday;

				((Gtk.Calendar)Wrapped).DisplayOptions = options;
			}
		}
	}
}
