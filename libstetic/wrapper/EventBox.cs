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
			groups = new ItemGroup[] {
				EventBoxProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new ItemGroup[0];
		}

		public EventBox (IStetic stetic) : this (stetic, new Gtk.EventBox ()) {}

		public EventBox (IStetic stetic, Gtk.EventBox eventbox) : base (stetic, eventbox) {}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }

		static ItemGroup[] childgroups;
		public override ItemGroup[] ChildItemGroups { get { return childgroups; } }
	}
}
