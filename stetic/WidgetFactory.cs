using Gtk;
using Gdk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class WidgetFactory : WidgetSite, IStetic {

		protected ConstructorInfo ctor;

		public WidgetFactory (string name, Pixbuf icon, Type wrapperType)
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

			this.ctor = wrapperType.GetConstructor (new Type[] { typeof (IStetic) });
			if (this.ctor == null)
				throw new ApplicationException ("No constructor for widget type " + wrapperType.ToString ());
		}

		protected override bool StartDrag (Gdk.EventMotion evt)
		{
			Stetic.Wrapper.Widget wrapper = ctor.Invoke (new object[] { this }) as Stetic.Wrapper.Widget;
			dragWidget = wrapper.Wrapped as Widget;
			return true;
		}

		public WidgetSite CreateWidgetSite ()
		{
			WidgetSite site = new WidgetSite ();
			site.PopupContextMenu += delegate (object obj, EventArgs args) {
				Menu m = new ContextMenu ((WidgetSite)site);
				m.Popup ();
			};
			return site;
		}
	}

	public class WindowFactory : WidgetFactory {
		public WindowFactory (string name, Pixbuf icon, Type wrapperType) :
			base (name, icon, wrapperType) {}

		protected override bool OnButtonPressEvent (Gdk.EventButton evt)
		{
			Stetic.Wrapper.Widget wrapper = ctor.Invoke (new object[] { this }) as Stetic.Wrapper.Widget;

			Gtk.Window win = wrapper.Wrapped as Gtk.Window;
			win.Present ();

			SteticMain.Project.AddWindow (win);

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
