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
			RegisterWrapper (typeof (Stetic.Wrapper.Frame),
					 FrameProperties,
					 Widget.CommonWidgetProperties);
		}

		public Frame (IStetic stetic) : this (stetic, new Gtk.Frame ()) {}

		public Frame (IStetic stetic, Gtk.Frame frame) : base (stetic, frame)
		{
			frame.Label = frame.Name;
		}
	}
}
