using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Frame", "frame.png", ObjectWrapperType.Container)]
	public class Frame : Bin {

		public static new Type WrappedType = typeof (Gtk.Frame);

		static new void Register (Type type)
		{
			AddItemGroup (type, "Frame Properties",
				      "Shadow",
				      "ShadowType",
				      "Label",
				      "LabelXalign",
				      "LabelYalign",
				      "BorderWidth");
		}

		protected override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			if (!initialized) {
				Gtk.Frame frame = (Gtk.Frame)Wrapped;
				frame.Label = frame.Name;
			}
		}
	}
}
