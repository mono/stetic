using System;

namespace Stetic.Wrapper {

	[ObjectWrapper ("Vertical Scale", "vscale.png", ObjectWrapperType.Widget)]
	public class VScale : Scale {

		public static new Type WrappedType = typeof (Gtk.VScale);

		public static new Gtk.VScale CreateInstance ()
		{
			return new Gtk.VScale (0.0, 100.0, 1.0);
		}

		public override bool VExpandable { get { return true; } }
	}
}
