using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Calendar", "calendar.png", ObjectWrapperType.Widget)]
	public class Calendar : Widget {

		public static new Type WrappedType = typeof (Gtk.Calendar);

		internal static new void Register (Type type)
		{
			AddItemGroup (type, "Calendar Properties",
				      "DisplayOptions/ShowHeading",
				      "DisplayOptions/ShowDayNames",
				      "DisplayOptions/ShowWeekNumbers",
				      "DisplayOptions/NoMonthChange");
		}

		Gtk.Calendar calendar {
			get {
				return (Gtk.Calendar)wrapped;
			}
		}

		[GladeProperty (Name = "display_options")]
		public Gtk.CalendarDisplayOptions DisplayOptions {
			get {
				return calendar.DisplayOptions;
			}
			set {
				calendar.DisplayOptions = value;
			}
		}
	}
}
