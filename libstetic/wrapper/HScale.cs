using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Horizontal Scale", "hscale.png", ObjectWrapperType.Widget)]
	public class HScale : Scale {

		public static new Type WrappedType = typeof (Gtk.HScale);

		public static new Gtk.HScale CreateInstance ()
		{
			return new Gtk.HScale (0.0, 100.0, 1.0);
		}

		public override bool HExpandable { get { return true; } }
	}
}
