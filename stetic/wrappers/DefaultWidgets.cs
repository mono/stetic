using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

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
			return new Stetic.Wrapper.Window ("Window");
		}

		[WidgetFactory ("Dialog Box", "dialog.png", WidgetType.Window)]
		static Widget newDialog ()
		{
			return new Gtk.Dialog ();
		}

		[WidgetFactory ("Frame", "frame.png", WidgetType.Container)]
		static Widget newFrame ()
		{
			return new Stetic.Wrapper.Frame ("Frame");
		}

		[WidgetFactory ("HBox", "hbox.png", WidgetType.Container)]
		static Widget newHBox ()
		{
			return new Stetic.Wrapper.HBox (false, 0);
		}

		[WidgetFactory ("VBox", "vbox.png", WidgetType.Container)]
		static Widget newVBox ()
		{
			return new Stetic.Wrapper.VBox (false, 0);
		}

		[WidgetFactory ("HButtonBox", "hbuttonbox.png", WidgetType.Container)]
		static Widget newHButtonBox ()
		{
			return new Stetic.Wrapper.HButtonBox ();
		}

		[WidgetFactory ("VButtonBox", "vbuttonbox.png", WidgetType.Container)]
		static Widget newVButtonBox ()
		{
			return new Stetic.Wrapper.VButtonBox ();
		}

		[WidgetFactory ("HPaned", "hpaned.png", WidgetType.Container)]
		static Widget newHPaned ()
		{
			return new Stetic.Wrapper.HPaned ();
		}
		[WidgetFactory ("VPaned", "vpaned.png", WidgetType.Container)]
		static Widget newVPaned ()
		{
			return new Stetic.Wrapper.VPaned ();
		}
#if NO
		[WidgetFactory ("Notebook", "notebook.png", WidgetType.Container)]
		static Widget newNotebook ()
		{
			return new Stetic.Wrapper.Notebook ();
		}
#endif

		[WidgetFactory ("Table", "table.png", WidgetType.Container)]
		static Widget newTable ()
		{
			return new Stetic.Wrapper.Table (3, 3, false);
		}
	}

}
