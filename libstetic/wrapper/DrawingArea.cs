using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Drawing Area", "drawingarea.png", typeof (Gtk.DrawingArea), ObjectWrapperType.Widget)]
	public class DrawingArea : Stetic.Wrapper.Widget {

		public DrawingArea (IStetic stetic) : this (stetic, new Gtk.DrawingArea (), false) {}
		public DrawingArea (IStetic stetic, Gtk.DrawingArea drawingarea, bool initialized) : base (stetic, drawingarea, initialized) {}
	}
}
