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

		static void ExtractWrapperProperties (ObjectWrapper wrapper, Hashtable props, 
						      out Hashtable wProps)
		{
			wProps = null;

			foreach (ItemGroup group in wrapper.ItemGroups) {
				foreach (ItemDescriptor item in group.Items) {
					PropertyDescriptor prop = item as PropertyDescriptor;
					if (prop == null || prop.GladeName == null)
						continue;
					if (prop.GladeFlags == 0 ||
					    (prop.GladeFlags & GladeProperty.UseUnderlying) != 0)
						continue;

					string val = ExtractProperty (prop.GladeName, props);
					if (val != null) {
						if (wProps == null)
							wProps = new Hashtable ();
						wProps[prop] = val;
					}
				}
			}
		}

		static void ParseProperty (IntPtr klass, bool childprop, string name, string strval, out GLib.Value value)
		{
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

					value = new GLib.Value (new Gtk.Adjustment (deflt, min, max, step, page_inc, page_size));
					return;
				} catch {
					;
				}
			}

			value = new GLib.Value ();
			bool parsed;

			if (childprop)
				parsed = stetic_g_value_parse_child_property (ref value, klass, name, strval);
			else
				parsed = stetic_g_value_parse_property (ref value, klass, name, strval);

			if (!parsed)
				throw new GladeException ("Could not parse property", GTypeNameFromClass (klass), childprop, name, strval);
		}

		static void ParseProperties (IntPtr gtype, bool childprops, Hashtable props,
					     out string[] propNames, out GLib.Value[] propVals)
		{
			IntPtr klass = g_type_class_ref (gtype);

			ArrayList names = new ArrayList ();
			ArrayList values = new ArrayList ();

			foreach (string name in props.Keys) {
				string strval = props[name] as string;

				GLib.Value value;
				try {
					ParseProperty (klass, childprops, name, strval, out value);
					names.Add (name);
					values.Add (value);
				} catch (GladeException ge) {
					Console.Error.WriteLine (ge.Message);
				}
			}

			propNames = (string[])names.ToArray (typeof (string));
			propVals = (GLib.Value[])values.ToArray (typeof (GLib.Value));

			g_type_class_unref (klass);
		}

		static public void ImportWidget (IStetic stetic, ObjectWrapper wrapper,
						 string className, string id, Hashtable props)
		{
			Hashtable wProps;
			ExtractWrapperProperties (wrapper, props, out wProps);

			IntPtr gtype = g_type_from_name (className);

			string[] propNames;
			GLib.Value[] propVals;
			ParseProperties (gtype, false, props, out propNames, out propVals);

			IntPtr raw = gtksharp_object_newv (gtype, propNames.Length, propNames, propVals);
			if (raw == IntPtr.Zero)
				throw new GladeException ("Could not create widget", className);

			Gtk.Widget widget = (Gtk.Widget)GLib.Object.GetObject (raw, true);
			if (widget == null) {
				gtk_object_sink (raw);
				throw new GladeException ("Could not create gtk# wrapper", className);
			}

			Wrap (stetic, wrapper, widget, id, wProps);
		}

		static public void ImportWidget (IStetic stetic, ObjectWrapper wrapper,
						 Gtk.Widget widget, string id, Hashtable props)
		{
			Hashtable wProps;
			ExtractWrapperProperties (wrapper, props, out wProps);

			string[] propNames;
			GLib.Value[] propVals;
			ParseProperties (gtksharp_get_type_id (widget.Handle), false, props,
					   out propNames, out propVals);

			for (int i = 0; i < propNames.Length; i++)
				g_object_set_property (widget.Handle, propNames[i], ref propVals[i]);

			Wrap (stetic, wrapper, widget, id, wProps);
		}

		static void Wrap (IStetic stetic, ObjectWrapper wrapper,
				  Gtk.Widget widget, string id, Hashtable wProps)
		{
			widget.Name = id;
			wrapper.Wrap (widget, true);

			if (wProps == null)
				return;

			IntPtr klass = g_type_class_ref (gtksharp_get_type_id (widget.Handle));
			LateImportHelper helper = null;

			foreach (PropertyDescriptor prop in wProps.Keys) {
				if ((prop.GladeFlags & GladeProperty.LateImport) != 0) {
					if (helper == null) {
						helper = new LateImportHelper (stetic, wrapper, klass, false);
						stetic.GladeImportComplete += helper.LateImport;
					}
					helper.Props[prop] = wProps[prop];
				} else
					ParseGladeProperty (wrapper, klass, false, prop, wProps[prop] as string);
			}

			if (helper == null)
				g_type_class_unref (klass);
		}

		private class LateImportHelper {
			public LateImportHelper (IStetic stetic, ObjectWrapper wrapper, IntPtr klass, bool childprop) {
				this.stetic = stetic;
				this.wrapper = wrapper;
				this.klass = klass;
				this.childprop = childprop;
				Props = new Hashtable ();
			}

			IStetic stetic;
			ObjectWrapper wrapper;
			IntPtr klass;
			bool childprop;
			public Hashtable Props;

			public void LateImport () {
				foreach (PropertyDescriptor prop in Props.Keys) {
					ParseGladeProperty (wrapper, klass, childprop,
							    prop, Props[prop] as string);
				}
				g_type_class_unref (klass);
				stetic.GladeImportComplete -= LateImport;
			}
		}

		static void ParseGladeProperty (ObjectWrapper wrapper, IntPtr klass, bool childprop,
						PropertyDescriptor prop, string strval)
		{
			if (((prop.GladeFlags & GladeProperty.Proxied) != 0) ||
			    prop.PropertyType == typeof (string))
				prop.GladeSetValue (wrapper, strval);
			else {
				GLib.Value value;
				ParseProperty (klass, childprop, prop.GladeName, strval, out value);
				if (prop.PropertyType.IsEnum) {
					GLib.EnumWrapper wrap = (GLib.EnumWrapper)value;
					prop.GladeSetValue (wrapper, Enum.ToObject (prop.PropertyType, (int)wrap));
				} else
					prop.GladeSetValue (wrapper, value.Val);
			}
		}

		static public void SetPacking (Gtk.Container parent, Gtk.Widget child, Hashtable childprops)
		{
			string[] propNames;
			GLib.Value[] propVals;
			ParseProperties (gtksharp_get_type_id (parent.Handle), true, childprops,
					 out propNames, out propVals);

			for (int i = 0; i < propNames.Length; i++)
				parent.ChildSetProperty (child, propNames[i], propVals[i]);
		}

		static string PropToString (ObjectWrapper wrapper, PropertyDescriptor prop)
		{
			GLib.Value gvalue;
			IntPtr raw;

			// If we're supposed to use the underlying property, just use that
			if ((prop.GladeFlags & GladeProperty.UseUnderlying) != 0) {
				Stetic.Wrapper.Container.ContainerChild ccwrap = wrapper as Stetic.Wrapper.Container.ContainerChild;
				if (ccwrap != null) {
					Gtk.Container.ContainerChild cc = (Gtk.Container.ContainerChild)ccwrap.Wrapped;
					raw = stetic_g_value_child_property_to_string (cc.Parent.Handle, cc.Child.Handle, prop.GladeName);
				} else {
					Gtk.Widget widget = wrapper.Wrapped as Gtk.Widget;
					raw = stetic_g_value_property_to_string (widget.Handle, prop.GladeName);
				}
				if (raw == IntPtr.Zero)
					return null;
				return GLib.Marshaller.PtrToStringGFree (raw);
			}

			// Otherwise, if this is a stetic-only property, skip it
			if (prop.GladeName == null && prop.GladeFlags == 0)
				return null;

			object value = prop.GladeGetValue (wrapper);
			if (value == null)
				return null;

			if (prop.GladeProperty != null)
				prop = prop.GladeProperty;

			// If there's no underlying property, just return the property
			// as a string. (This only works for some data types...)
			if (prop.ParamSpec == null)
				return value.ToString ();

			// If the property has its default value, we don't need to write it
			if (value.Equals (prop.ParamSpec.Default))
				return null;

			// Special handling for Adjustments
			if (value is Gtk.Adjustment) {
				Gtk.Adjustment adj = value as Gtk.Adjustment;
				return String.Format ("{0:G} {1:G} {2:G} {3:G} {4:G} {5:G}",
						      adj.Value, adj.Lower, adj.Upper,
						      adj.StepIncrement, adj.PageIncrement,
						      adj.PageSize);
			}

			if (prop.PropertyType.IsEnum) {
				string nativeEnumName = GTypeName (prop.ParamSpec.ValueType);
				GLib.EnumWrapper wrap = new GLib.EnumWrapper ((int)value, prop.PropertyType.IsDefined (typeof (FlagsAttribute), false));
				gvalue = new GLib.Value (wrap, nativeEnumName);
			} else
				gvalue = new GLib.Value (value);

			raw = stetic_g_value_to_string (ref gvalue);
			if (raw == IntPtr.Zero)
				throw new GladeException ("Could not convert property to string", ObjectWrapper.NativeTypeName (wrapper.GetType ()), false, prop.GladeName);

			return GLib.Marshaller.PtrToStringGFree (raw);
		}

		static public void ExportWidget (IStetic stetic, ObjectWrapper wrapper,
						 out string className, out string id, out Hashtable props)
		{
			Type wrappedType = ObjectWrapper.WrappedType (wrapper.GetType ());
			if (wrappedType == null)
				throw new GladeException ("Unrecognized wrapper type", wrapper.GetType ().ToString ());
			className = ObjectWrapper.NativeTypeName (wrappedType);

			id = ((Gtk.Widget)wrapper.Wrapped).Name;

			GetProps (wrapper, out props);
		}

		static public void GetProps (ObjectWrapper wrapper, out Hashtable props)
		{
			props = new Hashtable ();

			foreach (ItemGroup group in wrapper.ItemGroups) {
				foreach (ItemDescriptor item in group.Items) {
					PropertyDescriptor prop = item as PropertyDescriptor;
					if (prop == null || prop.GladeName == null)
						continue;

					string val = PropToString (wrapper, prop);
					if (val != null)
						props[prop.GladeName] = val;
				}
			}
		}

		static string GTypeName (IntPtr gtype)
		{
			return Marshal.PtrToStringAnsi (g_type_name (gtype));
		}

		static string GTypeNameFromClass (IntPtr klass)
		{
			return Marshal.PtrToStringAnsi (g_type_name_from_class (klass));
		}

		[DllImport("libsteticglue")]
		static extern bool stetic_g_value_parse_property (ref GLib.Value value, IntPtr klass, string propertyName, string data);

		[DllImport("libsteticglue")]
		static extern bool stetic_g_value_parse_child_property (ref GLib.Value value, IntPtr klass, string propertyName, string data);

		[DllImport("libsteticglue")]
		static extern IntPtr stetic_g_value_child_property_to_string (IntPtr parent, IntPtr child, string property_name);

		[DllImport("libsteticglue")]
		static extern IntPtr stetic_g_value_property_to_string (IntPtr widget, string property_name);

		[DllImport("libsteticglue")]
		static extern IntPtr stetic_g_value_to_string (ref GLib.Value value);

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_name (IntPtr gtype);

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_name_from_class (IntPtr klass);

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_from_name (string name);

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_class_ref (IntPtr gtype);

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_type_class_unref (IntPtr gtype);

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_object_newv (IntPtr gtype, int n_params, string[] names, GLib.Value[] vals);

		[DllImport("libgtk-win32-2.0-0.dll")]
		static extern void gtk_object_sink (IntPtr raw);

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_get_type_id (IntPtr obj);

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_set_property (IntPtr obj, string name, ref GLib.Value val);
	}
}
