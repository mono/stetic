using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Drawing Area", "drawingarea.png", ObjectWrapperType.Widget)]
	public class DrawingArea : Widget {

		public static new Type WrappedType = typeof (Gtk.DrawingArea);

		public override bool HExpandable { get { return true; } }
		public override bool VExpandable { get { return true; } }
	}
}
