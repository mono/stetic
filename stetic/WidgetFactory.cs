using Gtk;
using Gdk;
using System;
using System.Reflection;

namespace Stetic {

	public class WidgetFactory : WidgetSiteImpl, IStetic {

		protected Type widgetType;
		protected ConstructorInfo ctor;

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

			this.ctor = widgetType.GetConstructor (new Type[] { typeof (IStetic) });
			if (this.ctor == null)
				throw new ApplicationException ("No constructor for widget type " + widgetType.ToString ());
		}

		protected override bool StartDrag (Gdk.EventMotion evt)
		{
			dragWidget = ctor.Invoke (new object[] { this }) as Widget;
			dragWidget.ShowAll ();
			return true;
		}

		public WidgetSite CreateWidgetSite ()
		{
			return new WidgetSiteImpl ();
		}

		public WidgetSite CreateWidgetSite (int emptyWidth, int emptyHeight)
		{
			return new WidgetSiteImpl (emptyWidth, emptyHeight);
		}
	}

	public class WindowFactory : WidgetFactory {
		public WindowFactory (string name, Pixbuf icon, Type widgetType) :
			base (name, icon, widgetType) {}

		protected override bool OnButtonPressEvent (Gdk.EventButton evt)
		{
			Gtk.Window win = ctor.Invoke (new object[] { this }) as Gtk.Window;
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
