using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Drawing Area", "drawingarea.png", ObjectWrapperType.Widget)]
	public class DrawingArea : Widget {

		public static new Type WrappedType = typeof (Gtk.DrawingArea);
	}
}
