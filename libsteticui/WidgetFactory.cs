using Gtk;
using Gdk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class WidgetFactory : EventBox {

		protected Project project;
		protected ClassDescriptor klass;

		public WidgetFactory (Project project, ClassDescriptor klass)
		{
			this.project = project;
			this.klass = klass;
			DND.SourceSet (this);

			AboveChild = true;

			Gtk.HBox hbox = new HBox (false, 6);

			Gdk.Pixbuf icon = klass.Icon.ScaleSimple (16, 16, Gdk.InterpType.Bilinear);
			hbox.PackStart (new Gtk.Image (icon), false, false, 0);

			Gtk.Label label = new Gtk.Label ("<small>" + klass.Label + "</small>");
			label.UseMarkup = true;
			label.Justify = Justification.Left;
			label.Xalign = 0;
			hbox.PackEnd (label, true, true, 0);

			Add (hbox);
		}

		protected override void OnDragBegin (Gdk.DragContext ctx)
		{
			DND.Drag (this, ctx, klass.NewInstance (project) as Widget);
		}
	}

	public class WindowFactory : WidgetFactory {
		public WindowFactory (Project project, ClassDescriptor klass) : base (project, klass) {}

		protected override bool OnButtonPressEvent (Gdk.EventButton evt)
		{
			Gtk.Window win = klass.NewInstance (project) as Gtk.Window;
			project.AddWindow (win, true);
			return true;
		}
		
		protected override bool OnMotionNotifyEvent (Gdk.EventMotion evt)
		{
			return true;
		}
	}

}
