using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class Palette : Gtk.HBox {

		VBox normals, containers, windows;
		SizeGroup sgroup;

		public Palette () : base (false, 6)
		{
			BorderWidth = 6;

			normals = new VBox (true, 6);
			containers = new VBox (true, 6);
			windows = new VBox (true, 6);

			VBox right = new VBox (false, 12);

			PackStart (normals);
			PackStart (new VSeparator ());
			PackStart (right);

			sgroup = new SizeGroup (SizeGroupMode.Horizontal);
			sgroup.AddWidget (normals);
			sgroup.AddWidget (right);

			right.PackStart (containers, false, false, 0);
			right.PackStart (new HSeparator (), false, false, 0);
			right.PackStart (windows, false, false, 0);

			ShowAll ();
		}

		public void AddWidget (Assembly assem, Type type, VBox box, bool window)
		{
			foreach (object attr in type.GetCustomAttributes (typeof (WidgetWrapperAttribute), false)) {
				WidgetWrapperAttribute wwattr = attr as WidgetWrapperAttribute;
				Pixbuf icon;

				try {
					icon = new Gdk.Pixbuf (assem, wwattr.IconName);
				} catch {
					icon = Gdk.Pixbuf.LoadFromResource ("missing.png");
				}

				if (window)
					box.PackStart (new WindowFactory (wwattr.Name, icon, type), false, false, 0);
				else
					box.PackStart (new WidgetFactory (wwattr.Name, icon, type), false, false, 0);
			}
		}

		public void AddWidgets (Assembly assem)
		{
			foreach (Type type in assem.GetExportedTypes ()) {
				if (type.GetInterface ("Stetic.IWindowWrapper") != null)
					AddWidget (assem, type, windows, true);
				else if (type.GetInterface ("Stetic.IContainerWrapper") != null)
					AddWidget (assem, type, containers, false);
				else if (type.GetInterface ("Stetic.IObjectWrapper") != null)
					AddWidget (assem, type, normals, false);
			}

			ShowAll ();
		}			
	}
}
