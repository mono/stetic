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
			else {
				req.Width = req.Height = 200;
			}
		}

		protected override void OnSetFocus (Gtk.Widget focus)
		{
			WidgetSite oldf = Focus as WidgetSite;

			Gtk.Widget site = focus;
			while (site != null && !(site is WidgetSite))
				site = site.Parent;
			WidgetSite newf = (WidgetSite)site;

			if (oldf == newf)
				return;

			if (oldf != null)
				oldf.UnFocus ();

			if (newf != null)
				newf.Focus ();

			if (newf != null && newf.Child != null)
				Stetic.Select (newf);
			else
				Stetic.NoSelection ();

			base.OnSetFocus (focus);
		}
	}
}
