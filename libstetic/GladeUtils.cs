using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Stetic {

	public static class GladeUtils {

		public static string ExtractProperty (string name, ArrayList propNames, ArrayList propVals)
		{
			int index = propNames.IndexOf (name);
			if (index == -1)
				return null;

			string value = propVals[index] as string;
			propNames.RemoveAt (index);
			propVals.RemoveAt (index);
			return value;
		}

		[DllImport("libsteticglue")]
		static extern bool stetic_g_value_init_for_property (ref GLib.Value value, IntPtr gtype, string propertyName);

		[DllImport("libsteticglue")]
		static extern bool stetic_g_value_init_for_child_property (ref GLib.Value value, IntPtr gtype, string propertyName);

		[DllImport("libsteticglue")]
		static extern bool stetic_g_value_hydrate (ref GLib.Value value, string data);

		static bool Hydrate (IntPtr gtype, bool childprop, string name, string strval, out GLib.Value value)
		{
			value = new GLib.Value ();
			bool inited = false;

			if (childprop)
				inited = stetic_g_value_init_for_child_property (ref value, gtype, name);
			else
				inited = stetic_g_value_init_for_property (ref value, gtype, name);

			if (!inited) {
				Console.WriteLine ("Unrecognized {0}property name '{1}'",
						   childprop ? "child " : "", name);
				return false;
			}

			if (name == "adjustment") {
				try {
					string[] vals = strval.Split (' ');
					double deflt, min, max, step, page_inc, page_size;

					deflt = Double.Parse (vals[0]);
					min = Double.Parse (vals[1]);
					max = Double.Parse (vals[2]);
					step = Double.Parse (vals[3]);
					page_inc = Double.Parse (vals[4]);
					page_size = Double.Parse (vals[5]);

					value.Val = new Gtk.Adjustment (deflt, min, max, step, page_inc, page_size);
					return true;
				} catch {
					;
				}
			}

			if (!stetic_g_value_hydrate (ref value, strval)) {
				Console.WriteLine ("Could not hydrate {0}property '{1}' with value '{2}'",
						   childprop ? "child " : "", name, strval);
				return false;
			}

			return true;
		}

		static void HydrateProperties (IntPtr gtype, bool childprops, ArrayList names, ArrayList strvals, out ArrayList values)
		{
			values = new ArrayList ();

			int i = 0;
			while (i < names.Count) {
				GLib.Value value;

				if (Hydrate (gtype, childprops, names[i] as string, strvals[i] as string, out value)) {
					values.Add (value);
					i++;
				} else {
					names.RemoveAt (i);
					strvals.RemoveAt (i);
				}
			}
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_from_name (string name);

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

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_get_type_id (IntPtr obj);

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
