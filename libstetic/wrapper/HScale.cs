using System;

namespace Stetic.Wrapper {

	public class HScale : Scale {

		public static new Gtk.HScale CreateInstance ()
		{
			return new Gtk.HScale (0.0, 100.0, 1.0);
		}
	}
}
