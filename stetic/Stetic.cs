using Gtk;
using Gdk;
using System;

namespace Stetic {

	public class Stetic  {

		public static int Main (string[] args) {
			Gtk.Window win;
			Gtk.Widget widget;
			Gtk.Notebook notebook;
			Stetic.Palette palette;

			Application.Init ("Stetic", ref args);

			win = new Gtk.Window ("Palette");
			win.AllowGrow = false;
			win.DeleteEvent += Window_Delete;
			palette = new Stetic.Palette ();
			palette.AddWidgets (typeof (DefaultWidgets));
			win.Add (palette);
			win.ShowAll ();

			win = new Gtk.Window ("Properties");
			win.DeleteEvent += Window_Delete;
			notebook = new Gtk.Notebook ();
			win.Add (notebook);
			Properties = new Stetic.PropertyGrid ();
			Properties.Show ();
			notebook.AppendPage (Properties, new Label ("Properties"));
			ChildProperties = new Stetic.ChildPropertyGrid ();
			ChildProperties.Show ();
			notebook.AppendPage (ChildProperties, new Label ("Packing"));
			win.ShowAll ();

			Application.Run ();
			return 0;
		}

		static void Window_Delete (object obj, DeleteEventArgs args) {
			Application.Quit ();
			args.RetVal = true;
		}

		static Stetic.PropertyGrid Properties;
		static Stetic.ChildPropertyGrid ChildProperties;

		public static void NoSelection ()
		{
			Properties.NoSelection ();
			ChildProperties.NoSelection ();
		}

		public static void Select (WidgetBox wbox)
		{
			Properties.Select (wbox);
			ChildProperties.Select (wbox);
		}

	}
}
