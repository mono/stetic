using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Stetic {

	public static class GladeUtils {

		public static string ExtractProperty (string name, Hashtable props)
		{
			string value = props[name] as string;
			if (value != null)
				props.Remove (name);
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

		static void HydrateProperties (IntPtr gtype, bool childprops, Hashtable props,
					       out string[] propNames, out GLib.Value[] propVals)
		{
			ArrayList names = new ArrayList ();
			ArrayList values = new ArrayList ();

			foreach (string name in props.Keys) {
				string strval = props[name] as string;

				GLib.Value value;
				if (Hydrate (gtype, childprops, name, strval, out value)) {
					names.Add (name);
					values.Add (value);
				}
			}

			propNames = (string[])names.ToArray (typeof (string));
			propVals = (GLib.Value[])values.ToArray (typeof (GLib.Value));
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_from_name (string name);

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_object_newv (IntPtr gtype, int n_params, string[] names, GLib.Value[] vals);

		[DllImport("libgtk-win32-2.0-0.dll")]
		static extern void gtk_object_sink (IntPtr raw);

		static public Gtk.Widget CreateWidget (string className, Hashtable props)
		{
			IntPtr gtype = g_type_from_name (className);

			string[] propNames;
			GLib.Value[] propVals;
			HydrateProperties (gtype, false, props, out propNames, out propVals);

			IntPtr raw = gtksharp_object_newv (gtype, propNames.Length, propNames, propVals);
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

		static public void SetProps (Gtk.Widget widget, Hashtable props)
		{
			string[] propNames;
			GLib.Value[] propVals;
			HydrateProperties (gtksharp_get_type_id (widget.Handle), false, props,
					   out propNames, out propVals);

			for (int i = 0; i < propNames.Length; i++)
				g_object_set_property (widget.Handle, propNames[i], ref propVals[i]);
		}

		static public void SetPacking (Gtk.Container parent, Gtk.Widget child, Hashtable childprops)
		{
			string[] propNames;
			GLib.Value[] propVals;
			HydrateProperties (gtksharp_get_type_id (parent.Handle), true, childprops,
					   out propNames, out propVals);

			for (int i = 0; i < propNames.Length; i++)
				parent.ChildSetProperty (child, propNames[i], propVals[i]);
		}
	}
}
