using Gtk;
using Gdk;
using GLib;
using System;

namespace Stetic.Wrapper {

	static class DefaultWidgets {
		[WidgetFactory ("Button", "button.png")]
		static Gtk.Widget newButton () {
			return new Stetic.Wrapper.Button ();
		}

		[WidgetFactory ("Entry", "entry.png")]
		static Gtk.Widget newEntry ()
		{
			return new Stetic.Wrapper.Entry ();
		}

		[WidgetFactory ("Check Box", "checkbutton.png")]
		static Gtk.Widget newCheckButton ()
		{
			return new Stetic.Wrapper.CheckButton ();
		}

		[WidgetFactory ("Radio Button", "radiobutton.png")]
		static Gtk.Widget newRadioButton ()
		{
			return new Stetic.Wrapper.RadioButton ();
		}

		[WidgetFactory ("Spin Button", "spinbutton.png")]
		static Gtk.Widget newSpinButton ()
		{
			return new Gtk.SpinButton (null, 1.0, 2);
		}

		[WidgetFactory ("Label", "label.png")]
		static Gtk.Widget newLabel ()
		{
			return new Stetic.Wrapper.Label ();
		}

		[WidgetFactory ("HSeparator", "hseparator.png")]
		static Gtk.Widget newHSeparator ()
		{
			return new Stetic.Wrapper.HSeparator ();
		}

		[WidgetFactory ("VSeparator", "vseparator.png")]
		static Gtk.Widget newVSeparator ()
		{
			return new Stetic.Wrapper.VSeparator ();
		}

		[WidgetFactory ("Arrow", "arrow.png")]
		static Gtk.Widget newArrow ()
		{
			return new Stetic.Wrapper.Arrow ();
		}

		[WidgetFactory ("Combo Box", "combo.png")]
		static Gtk.Widget newComboBox ()
		{
			return new Stetic.Wrapper.ComboBox ();
		}

		[WidgetFactory ("Drawing Area", "drawingarea.png")]
		static Gtk.Widget newDrawingArea ()
		{
			return new Stetic.Wrapper.DrawingArea ();
		}

		[WidgetFactory ("Horizontal Scrollbar", "hscrollbar.png")]
		static Gtk.Widget newHScrollbar ()
		{
			return new Stetic.Wrapper.HScrollbar ();
		}

		[WidgetFactory ("Vertical Scrollbar", "vscrollbar.png")]
		static Gtk.Widget newVScrollbar ()
		{
			return new Stetic.Wrapper.VScrollbar ();
		}

		[WidgetFactory ("Image", "image.png")]
		static Gtk.Widget newImage ()
		{
			return new Gtk.Image ();
		}

		[WidgetFactory ("Text View", "textview.png")]
		static Gtk.Widget newTextView ()
		{
			return new Gtk.TextView ();
		}

		[WidgetFactory ("ToggleButton", "togglebutton.png")]
		static Gtk.Widget newToggleButton ()
		{
			return new Gtk.ToggleButton ("Toggle");
		}

		[WidgetFactory ("Window", "window.png", WidgetType.Window)]
		static Gtk.Widget newWindow ()
		{
			return new Stetic.Wrapper.Window ("Window");
		}

		[WidgetFactory ("Dialog Box", "dialog.png", WidgetType.Window)]
		static Gtk.Widget newDialog ()
		{
			return new Gtk.Dialog ();
		}

		[WidgetFactory ("Frame", "frame.png", WidgetType.Container)]
		static Gtk.Widget newFrame ()
		{
			return new Stetic.Wrapper.Frame ("Frame");
		}

		[WidgetFactory ("HBox", "hbox.png", WidgetType.Container)]
		static Gtk.Widget newHBox ()
		{
			return new Stetic.Wrapper.HBox (false, 0);
		}

		[WidgetFactory ("VBox", "vbox.png", WidgetType.Container)]
		static Gtk.Widget newVBox ()
		{
			return new Stetic.Wrapper.VBox (false, 0);
		}

		[WidgetFactory ("HButtonBox", "hbuttonbox.png", WidgetType.Container)]
		static Gtk.Widget newHButtonBox ()
		{
			return new Stetic.Wrapper.HButtonBox ();
		}

		[WidgetFactory ("VButtonBox", "vbuttonbox.png", WidgetType.Container)]
		static Gtk.Widget newVButtonBox ()
		{
			return new Stetic.Wrapper.VButtonBox ();
		}

		[WidgetFactory ("HPaned", "hpaned.png", WidgetType.Container)]
		static Gtk.Widget newHPaned ()
		{
			return new Stetic.Wrapper.HPaned ();
		}
		[WidgetFactory ("VPaned", "vpaned.png", WidgetType.Container)]
		static Gtk.Widget newVPaned ()
		{
			return new Stetic.Wrapper.VPaned ();
		}
#if NO
		[WidgetFactory ("Notebook", "notebook.png", WidgetType.Container)]
		static Gtk.Widget newNotebook ()
		{
			return new Stetic.Wrapper.Notebook ();
		}
#endif

		[WidgetFactory ("Table", "table.png", WidgetType.Container)]
		static Gtk.Widget newTable ()
		{
			return new Stetic.Wrapper.Table (3, 3, false);
		}
	}

}
