using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;

namespace Stetic {

	public class WindowWrapper : Gtk.Window {

		public WindowWrapper (string title) : base (title)
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

		protected override void OnSetFocus (Widget focus)
		{
			WidgetSite oldf = Focus as WidgetSite;

			Widget site = focus;
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
