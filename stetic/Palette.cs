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

		public void AddWidgets (Type type, WidgetType wtype, VBox box)
		{
			foreach (MethodInfo minfo in type.GetMethods (BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)) {
				if (minfo.ReturnType != typeof (Widget) || minfo.GetParameters ().Length != 0)
					continue;

				foreach (object attr in minfo.GetCustomAttributes (typeof (WidgetFactoryAttribute), false)) {
					WidgetFactoryAttribute wfattr = attr as WidgetFactoryAttribute;
					if (wfattr.Type != wtype)
						continue;

					WidgetFactoryDelegate deleg = Delegate.CreateDelegate (typeof (WidgetFactoryDelegate), minfo) as WidgetFactoryDelegate;
					if (deleg == null)
						continue;

					Pixbuf icon = new Gdk.Pixbuf (System.Reflection.Assembly.GetAssembly (type), wfattr.IconName);

					if (wtype == WidgetType.Window)
						box.PackStart (new WindowFactory (wfattr.Name, icon, deleg));
					else
						box.PackStart (new WidgetFactory (wfattr.Name, icon, deleg));
				}
			}

			ShowAll ();
		}

		public void AddWidgets (Type type)
		{
			AddWidgets (type, WidgetType.Normal, normals);
			AddWidgets (type, WidgetType.Container, containers);
			AddWidgets (type, WidgetType.Window, windows);
		}			

		public void AddWidgets (object obj)
		{
			AddWidgets (obj.GetType ());
		}

	}
}
