using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Drawing Area", "drawingarea.png", ObjectWrapperType.Widget)]
	public class DrawingArea : Stetic.Wrapper.Widget {

		public DrawingArea (IStetic stetic) : this (stetic, new Gtk.DrawingArea ()) {}
		public DrawingArea (IStetic stetic, Gtk.DrawingArea drawingarea) : base (stetic, drawingarea) {}
	}
}
