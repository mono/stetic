using System;
using System.Collections;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Event Box", "eventbox.png", ObjectWrapperType.Container)]
	public class EventBox : Bin {

		public static PropertyGroup EventBoxProperties;

		static EventBox () {
			EventBoxProperties = new PropertyGroup ("Event Box Properties",
								typeof (Gtk.EventBox),
								"AboveChild",
								"VisibleWindow",
								"BorderWidth");
			groups = new PropertyGroup[] {
				EventBoxProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[0];
		}

		public EventBox (IStetic stetic) : this (stetic, new Gtk.EventBox ()) {}

		public EventBox (IStetic stetic, Gtk.EventBox eventbox) : base (stetic, eventbox) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public override PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }
	}
}
