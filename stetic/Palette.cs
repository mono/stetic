using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class Palette : Gtk.VBox {

		VBox normals, containers, windows;

		public Palette () : base (false, 6)
		{
			BorderWidth = 6;

			normals = new VBox (true, 6);
			containers = new VBox (true, 6);
			windows = new VBox (true, 6);

			PackStart (normals);
			PackStart (new HSeparator ());
			PackStart (containers);
			PackStart (new HSeparator ());
			PackStart (windows);

			ShowAll ();
		}

		public void AddWidgets (Assembly assem, WidgetType wtype, VBox box)
		{
			foreach (Type type in assem.GetExportedTypes ()) {
				if (!type.IsSubclassOf (typeof (Gtk.Widget)) ||
				    type.GetConstructor (Type.EmptyTypes) == null)
					continue;

				foreach (object attr in type.GetCustomAttributes (typeof (WidgetWrapperAttribute), false)) {
					WidgetWrapperAttribute wwattr = attr as WidgetWrapperAttribute;
					if (wwattr.Type != wtype)
						continue;

					Pixbuf icon;

					try {
						icon = new Gdk.Pixbuf (assem, wwattr.IconName);
					} catch {
						icon = Gdk.Pixbuf.LoadFromResource ("missing.png");
					}

					if (wtype == WidgetType.Window)
						box.PackStart (new WindowFactory (wwattr.Name, icon, type));
					else
						box.PackStart (new WidgetFactory (wwattr.Name, icon, type));
				}
			}

			ShowAll ();
		}

		public void AddWidgets (Assembly assem)
		{
			AddWidgets (assem, WidgetType.Normal, normals);
			AddWidgets (assem, WidgetType.Container, containers);
			AddWidgets (assem, WidgetType.Window, windows);
		}			
	}
}
