using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic {

	public class WidgetFactory : WidgetSite {

		protected Type widgetType;

		public WidgetFactory (string name, Pixbuf icon, Type widgetType)
		{
			Gtk.HBox hbox;
			Gtk.Label label;

			CanFocus = false;

			hbox = new HBox (false, 6);

			icon = icon.ScaleSimple (16, 16, Gdk.InterpType.Bilinear);
			hbox.PackStart (new Gtk.Image (icon), false, false, 0);

			label = new Gtk.Label ("<small>" + name + "</small>");
			label.UseMarkup = true;
			label.Justify = Justification.Left;
			label.Xalign = 0;
			hbox.PackEnd (label, true, true, 0);

			Add (hbox);
			this.widgetType = widgetType;
		}

		protected override bool StartDrag ()
		{
			dragWidget = System.Activator.CreateInstance (widgetType) as Widget;
			dragWidget.ShowAll ();
			return true;
		}
	}

	public class WindowFactory : WidgetFactory {
		public WindowFactory (string name, Pixbuf icon, Type widgetType) :
			base (name, icon, widgetType) {}

		protected override bool OnButtonPressEvent (Gdk.EventButton evt)
		{
			Gtk.Window win = System.Activator.CreateInstance (widgetType) as Gtk.Window;
			win.Present ();

			WindowSite site = new WindowSite (win);
			SteticMain.Select (site);
			site.FocusChanged += delegate (WindowSite site, IWidgetSite focus) {
				if (focus == null)
					SteticMain.NoSelection ();
				else
					SteticMain.Select (focus);
			};
			return true;
		}

		protected override bool OnMotionNotifyEvent (Gdk.EventMotion evt)
		{
			return true;
		}
	}

}
