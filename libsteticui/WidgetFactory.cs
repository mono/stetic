using Gtk;
using Gdk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	// This is the base class for palette items. Implements the basic
	// functionality for showing the icon and label of the item.
	
	public class PaletteItemFactory : EventBox 
	{
		public PaletteItemFactory ()
		{
		}
		
		public void Initialize (string name, Gdk.Pixbuf icon)
		{
			DND.SourceSet (this);
			AboveChild = true;

			Gtk.HBox hbox = new HBox (false, 6);

			if (icon != null) {
				icon = icon.ScaleSimple (16, 16, Gdk.InterpType.Bilinear);
				hbox.PackStart (new Gtk.Image (icon), false, false, 0);
			}

			Gtk.Label label = new Gtk.Label ("<small>" + name + "</small>");
			label.UseMarkup = true;
			label.Justify = Justification.Left;
			label.Xalign = 0;
			hbox.PackEnd (label, true, true, 0);

			Add (hbox);
		}

		protected override void OnDragBegin (Gdk.DragContext ctx)
		{
			Gtk.Widget ob = CreateItemWidget ();
			if (ob != null)
				DND.Drag (this, ctx, ob);
		}
		
		protected virtual Gtk.Widget CreateItemWidget ()
		{
			return null;
		}
	}


	// Palette item factory which creates a widget.
	public class WidgetFactory : PaletteItemFactory {

		protected Project project;
		protected ClassDescriptor klass;

		public WidgetFactory (Project project, ClassDescriptor klass)
		{
			this.project = project;
			this.klass = klass;
			Initialize (klass.Label, klass.Icon);
		}

		protected override Gtk.Widget CreateItemWidget ()
		{
			return klass.NewInstance (project) as Widget;
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
	
	// Palette item factory which allows dragging an already existing object.
	public class InstanceWidgetFactory : PaletteItemFactory 
	{
		Gtk.Widget instance;
		
		public InstanceWidgetFactory (string name, Gdk.Pixbuf icon, Gtk.Widget instance)
		{
			this.instance = instance;
			Initialize (name, icon);
		}

		protected override Gtk.Widget CreateItemWidget ()
		{
			return instance;
		}
	}
}
