using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Event Box", "eventbox.png", ObjectWrapperType.Container)]
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

		public EventBox (IStetic stetic) : this (stetic, new Gtk.EventBox ()) {}
		public EventBox (IStetic stetic, Gtk.EventBox eventbox) : base (stetic, eventbox) {}
	}
}
