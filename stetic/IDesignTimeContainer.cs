using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic {

	public delegate void OccupancyChangedHandler (IDesignTimeContainer holder);

	public interface IDesignTimeContainer {
		bool HExpandable { get; }
		bool VExpandable { get; }

		event OccupancyChangedHandler OccupancyChanged;
	}

}
