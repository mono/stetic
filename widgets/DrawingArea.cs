using Gtk;
using System;

namespace Stetic.Widget {

	[WidgetWrapper ("Drawing Area", "drawingarea.png")]
	public class DrawingArea : Gtk.DrawingArea, Stetic.IWidgetWrapper {
		static PropertyGroup[] groups;
		public PropertyGroup[] PropertyGroups { get { return groups; } }

		static DrawingArea () {
			groups = new PropertyGroup[] {
				Widget.CommonWidgetProperties
			};
		}

		public DrawingArea (IStetic stetic) : base () {}
	}
}
