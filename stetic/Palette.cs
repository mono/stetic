using Gtk;
using Gdk;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {

	public class Palette : Gtk.VBox {

		VBox normals, containers, windows;
		SizeGroup sgroup;
		ComboBox combo;
		ListStore model;

		Hashtable widgets = new Hashtable ();

		public Palette () : base (false, 6)
		{
			BorderWidth = 6;

			normals = new VBox (false, 0);
			containers = new VBox (false, 0);
			windows = new VBox (false, 0);
			
			combo = new ComboBox ();
			model = new ListStore (typeof (string));
			combo.Model = model;
			CellRendererText text_renderer = new CellRendererText ();
			combo.PackStart (text_renderer, true);
			combo.AddAttribute (text_renderer, "text", 0);
			combo.Changed += new EventHandler (OnSelectorChanged);

			PackStart (combo, false, false, 3);
			PackStart (new Gtk.VBox (false, 0), true, true, 3);

			ShowAll ();
		}

		void OnSelectorChanged (object o, EventArgs e)
		{
			Remove (Children[1]);
			TreeIter iter;
			if (combo.GetActiveIter (out iter)) {
				PackStart ((Widget)widgets[(string)model.GetValue (iter, 0)], true, true, 3);
			}
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
				ObjectWrapper.RegisterWrapperType (type, owattr.WrappedType);
				Pixbuf icon;

				try {
					icon = new Gdk.Pixbuf (assem, owattr.IconName);
				} catch {
					icon = Gdk.Pixbuf.LoadFromResource ("missing.png");
				}

				Widget w;
				string catalogue;
				
				switch (owattr.Type) {
				case ObjectWrapperType.Container:
					w = new WidgetFactory (owattr.Name, icon, type);
					catalogue = "GTK# Containers";
					break;
				case ObjectWrapperType.Window:
					w = new WindowFactory (owattr.Name, icon, type);
					catalogue = "GTK# Windows";
					break;
				default:
					w = new WidgetFactory (owattr.Name, icon, type);
					catalogue = "GTK# Widgets";
					break;
				}
				if (!widgets.Contains (catalogue)) {
					ScrolledWindow scroller = new ScrolledWindow ();
					scroller.AddWithViewport (new Gtk.VBox (false, 0));
					scroller.ShadowType = ShadowType.None;
					scroller.HscrollbarPolicy = PolicyType.Never;
					widgets[catalogue] = scroller;
					model.AppendValues (catalogue);
					if (combo.Active == -1)
						combo.Active = 0;
				}
				((Box)((Bin)((Bin)widgets[catalogue]).Child).Child).PackStart (w, false, false, 0);
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
