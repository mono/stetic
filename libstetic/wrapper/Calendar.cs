using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Calendar", "calendar.png", ObjectWrapperType.Widget)]
	public class Calendar : Widget {

		public static new Type WrappedType = typeof (Gtk.Calendar);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Calendar Properties",
				      "ShowHeading",
				      "ShowDayNames",
				      "ShowWeekNumbers",
				      "NoMonthChange");
		}
	}
}
