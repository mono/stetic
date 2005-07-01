using System;

namespace Stetic.Wrapper {

	public class VScale : Scale {

		public static new Gtk.VScale CreateInstance ()
		{
			return new Gtk.VScale (0.0, 100.0, 1.0);
		}
	}
}
