using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Event Box", "eventbox.png", typeof (Gtk.EventBox), ObjectWrapperType.Container)]
	public class EventBox : Bin {

		public static ItemGroup EventBoxProperties;

		static EventBox () {
			EventBoxProperties = new ItemGroup ("Event Box Properties",
							    typeof (Gtk.EventBox),
							    "AboveChild",
							    "VisibleWindow",
							    "BorderWidth");
			RegisterWrapper (typeof (Stetic.Wrapper.EventBox),
					 EventBoxProperties,
					 Widget.CommonWidgetProperties);
		}

		public EventBox (IStetic stetic) : this (stetic, new Gtk.EventBox (), false) {}
		public EventBox (IStetic stetic, Gtk.EventBox eventbox, bool initialized) : base (stetic, eventbox, initialized) {}
	}
}
