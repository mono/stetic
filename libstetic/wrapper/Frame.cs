using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Frame", "frame.png", ObjectWrapperType.Container)]
	public class Frame : Bin {

		public static PropertyGroup FrameProperties;

		static Frame () {
			FrameProperties = new PropertyGroup ("Frame Properties",
							     typeof (Gtk.Frame),
							     "Shadow",
							     "ShadowType",
							     "Label",
							     "LabelXalign",
							     "LabelYalign",
							     "BorderWidth");

			groups = new PropertyGroup[] {
				FrameProperties,
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};

			childgroups = new PropertyGroup[0];
		}

		public Frame (IStetic stetic) : this (stetic, new Gtk.Frame ("Frame")) {}

		public Frame (IStetic stetic, Gtk.Frame frame) : base (stetic, frame) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }

		static PropertyGroup[] childgroups;
		public override PropertyGroup[] ChildPropertyGroups { get { return childgroups; } }
	}
}
