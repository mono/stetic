using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	public class Window : Gtk.Window {

		public Window (string title) : base (title)
		{
			WidgetSite site = new WidgetSite ();
			site.Show ();
			Add (site);
		}

		protected override void OnSizeRequested (ref Requisition req)
		{
			WidgetSite site = (WidgetSite)Child;

			if (site.Occupied)
				req = site.SizeRequest ();
			else
				req.Width = req.Height = 200;
		}
	}
}
