using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic {

	public delegate Widget WidgetFactoryDelegate ();

	public enum WidgetType {
		Normal,
		Container,
		Window
	};

	public class WidgetFactoryAttribute : Attribute {
		string name, iconName;
		WidgetType type;

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string IconName {
			get { return iconName; }
			set { iconName = value; }
		}

		public WidgetType Type {
			get { return type; }
			set { type = value; }
		}

		public WidgetFactoryAttribute (string name, string iconName, WidgetType type)
		{
			this.name = name;
			this.iconName = iconName;
			this.type = type;
		}

		public WidgetFactoryAttribute (string name, string iconName) : this (name, iconName, WidgetType.Normal) {}
	}

	public class WidgetFactory : WidgetSite {

		protected WidgetFactoryDelegate makeWidget;

		public WidgetFactory (string name, Pixbuf icon,
				      WidgetFactoryDelegate makeWidget) : base ()
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
			this.makeWidget = makeWidget;
		}

		protected override bool StartDrag ()
		{
			dragWidget = makeWidget ();
			dragWidget.ShowAll ();
			return true;
		}
	}

	public class WindowFactory : WidgetFactory {
		public WindowFactory (string name, Pixbuf icon,
				      WidgetFactoryDelegate makeWidget) : base (name, icon, makeWidget) {}

		protected override bool OnButtonPressEvent (Gdk.EventButton evt)
		{
			Gtk.Window win = makeWidget () as Gtk.Window;
			win.Present ();

			WindowSite site = new WindowSite (win);
			Stetic.Select (site);
			site.FocusChanged += delegate (WindowSite site, IWidgetSite focus) {
				if (focus == null)
					Stetic.NoSelection ();
				else
					Stetic.Select (focus);
			};
			return true;
		}

		protected override bool OnMotionNotifyEvent (Gdk.EventMotion evt)
		{
			return true;
		}
	}

}
