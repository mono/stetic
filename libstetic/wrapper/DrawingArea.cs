using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Drawing Area", "drawingarea.png", ObjectWrapperType.Widget)]
	public class DrawingArea : Stetic.Wrapper.Widget {

		static DrawingArea () {
			groups = new PropertyGroup[] {
				Stetic.Wrapper.Widget.CommonWidgetProperties
			};
		}

		public DrawingArea (IStetic stetic) : this (stetic, new Gtk.DrawingArea ()) {}

		public DrawingArea (IStetic stetic, Gtk.DrawingArea drawingarea) : base (stetic, drawingarea) {}

		static PropertyGroup[] groups;
		public override PropertyGroup[] PropertyGroups { get { return groups; } }
	}
}
