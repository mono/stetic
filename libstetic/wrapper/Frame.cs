using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Frame", "frame.png", ObjectWrapperType.Container)]
	public class Frame : Bin {

		public static ItemGroup FrameProperties;

		static Frame () {
			FrameProperties = new ItemGroup ("Frame Properties",
							 typeof (Gtk.Frame),
							 "Shadow",
							 "ShadowType",
							 "Label",
							 "LabelXalign",
							 "LabelYalign",
							 "BorderWidth");

			groups = new ItemGroup[] {
				FrameProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new ItemGroup[0];
		}

		public Frame (IStetic stetic) : this (stetic, new Gtk.Frame ()) {}

		public Frame (IStetic stetic, Gtk.Frame frame) : base (stetic, frame)
		{
			frame.Label = frame.Name;
		}

		static ItemGroup[] groups;
		public override ItemGroup[] ItemGroups { get { return groups; } }

		static ItemGroup[] childgroups;
		public override ItemGroup[] ChildItemGroups { get { return childgroups; } }
	}
}
