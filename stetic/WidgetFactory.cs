using Gtk;
using Gdk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class WidgetFactory : WidgetSiteImpl, IStetic {

		ConstructorInfo ctor;
		string basename;
		int counter;

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

			basename = wrapperType.Name.ToLower ();
			counter = 1;
		}

		protected Widget Create ()
		{
			Stetic.Wrapper.Object wrapper = ctor.Invoke (new object[] { this }) as Stetic.Wrapper.Object;
			Widget w = wrapper.Wrapped as Widget;

			w.Name = basename + (counter++).ToString ();
			return w;
		}

		protected override bool StartDrag (Gdk.EventMotion evt)
		{
			dragWidget = Create ();
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
		public WindowFactory (string name, Pixbuf icon, Type wrapperType) :
			base (name, icon, wrapperType) {}

		protected override bool OnButtonPressEvent (Gdk.EventButton evt)
		{
			Gtk.Window win = Create () as Gtk.Window;
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
