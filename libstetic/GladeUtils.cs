using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Stetic {

	public static class GladeUtils {

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_from_name (string name);

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_get_type_id (IntPtr obj);

		[DllImport("libsteticglue")]
		static extern bool stetic_g_value_init_for_property (ref GLib.Value value, IntPtr gtype, string propertyName);

		[DllImport("libsteticglue")]
		static extern bool stetic_g_value_init_for_child_property (ref GLib.Value value, IntPtr gtype, string propertyName);

		[DllImport("libsteticglue")]
		static extern bool stetic_g_value_hydrate (ref GLib.Value value, string data);

		static void HydrateProperties (IntPtr gtype, bool childprops, ArrayList names, ArrayList strvals, out ArrayList values)
		{
			values = new ArrayList ();

			int i = 0;
			while (i < names.Count) {
				string name = names[i] as string;
				string strval = strvals[i] as string;

				GLib.Value value = new GLib.Value ();
				bool inited = false;

				if (childprops)
					inited = stetic_g_value_init_for_child_property (ref value, gtype, name);
				else
					inited = stetic_g_value_init_for_property (ref value, gtype, name);

				if (!inited || !stetic_g_value_hydrate (ref value, strval)) {
					names.RemoveAt (i);
					strvals.RemoveAt (i);
				} else {
					values.Add (value);
					i++;
				}
			}
		}

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_object_newv (IntPtr gtype, int n_params, string[] names, GLib.Value[] vals);

		[DllImport("libgtk-win32-2.0-0.dll")]
		static extern void gtk_object_sink (IntPtr raw);

		static public Gtk.Widget CreateWidget (string className, ArrayList propNames, ArrayList propStrVals)
		{
			IntPtr gtype = g_type_from_name (className);

			ArrayList propVals;
			HydrateProperties (gtype, false, propNames, propStrVals, out propVals);

			string[] propNamesArray = (string[])propNames.ToArray (typeof (string));
			GLib.Value[] propValsArray = (GLib.Value[])propVals.ToArray (typeof (GLib.Value));

			IntPtr raw = gtksharp_object_newv (gtype, propNamesArray.Length, propNamesArray, propValsArray);
			if (raw == IntPtr.Zero) {
				Console.WriteLine ("Could not create widget of type {0}", className);
				return null;
			}

			Gtk.Widget widget = (Gtk.Widget)GLib.Object.GetObject (raw, true);
			if (widget == null) {
				Console.WriteLine ("Could not create gtk# wrapper for type {0}", className);
				gtk_object_sink (raw);
			}

			return widget;
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_set_property (IntPtr obj, string name, ref GLib.Value val);

		static public void SetProps (Gtk.Widget widget, ArrayList propNames, ArrayList propStrVals)
		{
			ArrayList propVals;
			HydrateProperties (gtksharp_get_type_id (widget.Handle), false, propNames, propStrVals, out propVals);

			for (int i = 0; i < propNames.Count; i++) {
				GLib.Value value = (GLib.Value)propVals[i];
				g_object_set_property (widget.Handle, (string)propNames[i], ref value);
			}
		}

		static public void SetPacking (Gtk.Container parent, Gtk.Widget child,
					       ArrayList propNames, ArrayList propStrVals)
		{
			ArrayList propVals;
			HydrateProperties (gtksharp_get_type_id (parent.Handle), true, propNames, propStrVals, out propVals);

			for (int i = 0; i < propNames.Count; i++)
				parent.ChildSetProperty (child, (string)propNames[i], (GLib.Value)propVals[i]);
		}
	}
}
