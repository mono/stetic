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
			return true;
		}

		protected override bool OnMotionNotifyEvent (Gdk.EventMotion evt)
		{
			return true;
		}
	}

	static class DefaultWidgets {
		[WidgetFactory ("Button", "button.png")]
		static Widget newButton () {
			return new Gtk.Button (Gtk.Stock.Ok);
		}

		[WidgetFactory ("Entry", "entry.png")]
		static Widget newEntry ()
		{
			return new Gtk.Entry ();
		}

		[WidgetFactory ("Check Box", "checkbutton.png")]
		static Widget newCheckButton ()
		{
			return new Gtk.CheckButton ("Do something");
		}

		[WidgetFactory ("Radio Button", "radiobutton.png")]
		static Widget newRadioButton ()
		{
			return new Gtk.RadioButton ("Do something");
		}

		[WidgetFactory ("Spin Button", "spinbutton.png")]
		static Widget newSpinButton ()
		{
			return new Gtk.SpinButton (null, 1.0, 2);
		}

		[WidgetFactory ("Label", "label.png")]
		static Widget newLabel ()
		{
			return new Gtk.Label ("Label:");
		}

		[WidgetFactory ("HSeparator", "hseparator.png")]
		static Widget newHSeparator ()
		{
			return new Gtk.HSeparator ();
		}

		[WidgetFactory ("VSeparator", "vseparator.png")]
		static Widget newVSeparator ()
		{
			return new Gtk.VSeparator ();
		}

		[WidgetFactory ("Arrow", "arrow.png")]
		static Widget newArrow ()
		{
			return new Gtk.Arrow (ArrowType.Left, ShadowType.None);
		}

		[WidgetFactory ("Combo Box", "combo.png")]
		static Widget newComboBox ()
		{
			return new Gtk.ComboBox ();
		}

		[WidgetFactory ("Drawing Area", "drawingarea.png")]
		static Widget newDrawingArea ()
		{
			return new Gtk.DrawingArea ();
		}

		[WidgetFactory ("Horizontal Scrollbar", "hscrollbar.png")]
		static Widget newHScrollbar ()
		{
			return new Gtk.HScrollbar (new Gtk.Adjustment (0.0, 0.0, 100.0, 1.0, 10.0, 10.0));
		}

		[WidgetFactory ("Vertical Scrollbar", "vscrollbar.png")]
		static Widget newVScrollbar ()
		{
			return new Gtk.VScrollbar (new Gtk.Adjustment (0.0, 0.0, 100.0, 1.0, 10.0, 10.0));
		}

		[WidgetFactory ("Image", "image.png")]
		static Widget newImage ()
		{
			return new Gtk.Image ();
		}

		[WidgetFactory ("Text View", "textview.png")]
		static Widget newTextView ()
		{
			return new Gtk.TextView ();
		}

		[WidgetFactory ("ToggleButton", "togglebutton.png")]
		static Widget newToggleButton ()
		{
			return new Gtk.ToggleButton ("Toggle");
		}

		[WidgetFactory ("Window", "window.png", WidgetType.Window)]
		static Widget newWindow ()
		{
			return new WindowWrapper ("Window");
		}

		[WidgetFactory ("Dialog Box", "dialog.png", WidgetType.Window)]
		static Widget newDialog ()
		{
			return new Gtk.Dialog ();
		}

		[WidgetFactory ("Frame", "frame.png", WidgetType.Container)]
		static Widget newFrame ()
		{
			return new FrameWrapper ("Frame");
		}

		[WidgetFactory ("HBox", "hbox.png", WidgetType.Container)]
		static Widget newHBox ()
		{
			return new HBoxWrapper (false, 0);
		}

		[WidgetFactory ("VBox", "vbox.png", WidgetType.Container)]
		static Widget newVBox ()
		{
			return new VBoxWrapper (false, 0);
		}

		[WidgetFactory ("HButtonBox", "hbuttonbox.png", WidgetType.Container)]
		static Widget newHButtonBox ()
		{
			return new HButtonBoxWrapper ();
		}

		[WidgetFactory ("VButtonBox", "vbuttonbox.png", WidgetType.Container)]
		static Widget newVButtonBox ()
		{
			return new VButtonBoxWrapper ();
		}

		[WidgetFactory ("HPaned", "hpaned.png", WidgetType.Container)]
		static Widget newHPaned ()
		{
			return new HPanedWrapper ();
		}
		[WidgetFactory ("VPaned", "vpaned.png", WidgetType.Container)]
		static Widget newVPaned ()
		{
			return new VPanedWrapper ();
		}
#if NO
		[WidgetFactory ("Notebook", "notebook.png", WidgetType.Container)]
		static Widget newNotebook ()
		{
			return new NotebookWrapper ();
		}
#endif

		[WidgetFactory ("Table", "table.png", WidgetType.Container)]
		static Widget newTable ()
		{
			return new TableWrapper (3, 3, false);
		}
	}

}
