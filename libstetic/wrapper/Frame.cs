using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Frame", "frame.png", typeof (Gtk.Frame), ObjectWrapperType.Container)]
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

		public Frame (IStetic stetic) : this (stetic, new Gtk.Frame (), false) {}


		public Frame (IStetic stetic, Gtk.Frame frame, bool initialized) : base (stetic, frame, initialized)
		{
			if (!initialized) {
				frame.Label = frame.Name;
			}
		}
	}
}
