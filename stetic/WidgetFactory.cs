using Gtk;
using Gdk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class WidgetFactory : EventBox {

		protected Project project;
		protected Type wrapperType;

		public WidgetFactory (Project project, string name, Pixbuf icon, Type wrapperType)
		{
			this.project = project;
			this.wrapperType = wrapperType;
			DND.SourceSet (this, true);

			AboveChild = true;

			Gtk.HBox hbox = new HBox (false, 6);

			icon = icon.ScaleSimple (16, 16, Gdk.InterpType.Bilinear);
			hbox.PackStart (new Gtk.Image (icon), false, false, 0);

			Gtk.Label label = new Gtk.Label ("<small>" + name + "</small>");
			label.UseMarkup = true;
			label.Justify = Justification.Left;
			label.Xalign = 0;
			hbox.PackEnd (label, true, true, 0);

			Add (hbox);
		}

		protected override void OnDragBegin (Gdk.DragContext ctx)
		{
			Stetic.Wrapper.Widget wrapper = Stetic.ObjectWrapper.Create (project, wrapperType) as Stetic.Wrapper.Widget;
			DND.Drag (this, ctx, wrapper.Wrapped as Widget);
		}
	}

	public class WindowFactory : WidgetFactory {
		public WindowFactory (Project project, string name, Pixbuf icon, Type wrapperType) :
			base (project, name, icon, wrapperType) {}

		protected override bool OnButtonPressEvent (Gdk.EventButton evt)
		{
			Stetic.Wrapper.Window wrapper = Stetic.ObjectWrapper.Create (project, wrapperType) as Stetic.Wrapper.Window;

			Gtk.Window win = wrapper.Wrapped as Gtk.Window;
			win.Present ();

			project.AddWindow (wrapper, true);

			return true;
		}

		protected override bool OnMotionNotifyEvent (Gdk.EventMotion evt)
		{
			return true;
		}
	}

}
