using Gtk;
using Gdk;
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

		public static Pixbuf IconForType (Type type)
		{
			foreach (object attr in type.GetCustomAttributes (typeof (ObjectWrapperAttribute), false)) {
				ObjectWrapperAttribute owattr = attr as ObjectWrapperAttribute;

				try {
					return new Gdk.Pixbuf (type.Assembly, owattr.IconName);
				} catch {
					;
				}
			}
			return Gdk.Pixbuf.LoadFromResource ("missing.png");
		}

		public void AddWidget (Assembly assem, Type type)
		{
			foreach (object attr in type.GetCustomAttributes (typeof (ObjectWrapperAttribute), false)) {
				ObjectWrapperAttribute owattr = attr as ObjectWrapperAttribute;
				Pixbuf icon;

				try {
					icon = new Gdk.Pixbuf (assem, owattr.IconName);
				} catch {
					icon = Gdk.Pixbuf.LoadFromResource ("missing.png");
				}

				switch (owattr.Type) {
				case ObjectWrapperType.Container:
					containers.PackStart (new WidgetFactory (owattr.Name, icon, type), false, false, 0);
					break;

				case ObjectWrapperType.Window:
					windows.PackStart (new WindowFactory (owattr.Name, icon, type), false, false, 0);
					break;

				default:
					normals.PackStart (new WidgetFactory (owattr.Name, icon, type), false, false, 0);
					break;
				}
			}
		}

		public void AddWidgets (Assembly assem)
		{
			foreach (Type type in assem.GetExportedTypes ())
				AddWidget (assem, type);

			ShowAll ();
		}			
	}
}
