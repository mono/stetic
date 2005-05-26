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
					if (prop == null)
						continue;
					prop = prop.GladeProperty;
					if (prop.GladeName == null)
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

		static GLib.Value ParseBasicType (GLib.TypeFundamentals type, string strval)
		{
			switch (type) {
			case GLib.TypeFundamentals.TypeChar:
				return new GLib.Value (SByte.Parse (strval));
			case GLib.TypeFundamentals.TypeUChar:
				return new GLib.Value (Byte.Parse (strval));
			case GLib.TypeFundamentals.TypeBoolean:
				return new GLib.Value (strval == "True");
			case GLib.TypeFundamentals.TypeInt:
				return new GLib.Value (Int32.Parse (strval));
			case GLib.TypeFundamentals.TypeUInt:
				return new GLib.Value (UInt32.Parse (strval));
			case GLib.TypeFundamentals.TypeInt64:
				return new GLib.Value (Int64.Parse (strval));
			case GLib.TypeFundamentals.TypeUInt64:
				return new GLib.Value (UInt64.Parse (strval));
			case GLib.TypeFundamentals.TypeFloat:
				return new GLib.Value (Single.Parse (strval));
			case GLib.TypeFundamentals.TypeDouble:
				return new GLib.Value (Double.Parse (strval));
			case GLib.TypeFundamentals.TypeString:
				return new GLib.Value (strval);
			default:
				throw new GladeException ("Could not parse");
			}
		}

		static GLib.Value ParseEnum (IntPtr gtype, string strval)
		{
			IntPtr enum_class = g_type_class_ref (gtype);
			try {
				IntPtr enum_value = g_enum_get_value_by_name (enum_class, strval);
				if (enum_value == IntPtr.Zero)
					throw new GladeException ("Could not parse");

				IntPtr eval = Marshal.ReadIntPtr (enum_value);

				string typeName = GLib.Marshaller.Utf8PtrToString (g_type_name (gtype));
				return new GLib.Value (new GLib.EnumWrapper ((int)eval, false), typeName);
			} finally {
				g_type_class_unref (enum_class);
			}
		}

		static GLib.Value ParseFlags (IntPtr gtype, string strval)
		{
			IntPtr flags_class = g_type_class_ref (gtype);
			uint fval = 0;

			try {
				// The Trim() is needed for Widget.Events.
				foreach (string flag in strval.Split ('|')) {
					if (flag == "")
						continue;
					IntPtr flags_value = g_flags_get_value_by_name (flags_class, flag.Trim ());
					if (flags_value == IntPtr.Zero)
						throw new GladeException ("Could not parse");

					IntPtr bits = Marshal.ReadIntPtr (flags_value);
					fval |= (uint)bits;
				}

				string typeName = GLib.Marshaller.Utf8PtrToString (g_type_name (gtype));
				return new GLib.Value (new GLib.EnumWrapper ((int)fval, true), typeName);
			} finally {
				g_type_class_unref (flags_class);
			}
		}

		static GLib.Value ParseAdjustment (string strval)
		{
			string[] vals = strval.Split (' ');
			double deflt, min, max, step, page_inc, page_size;

			deflt = Double.Parse (vals[0]);
			min = Double.Parse (vals[1]);
			max = Double.Parse (vals[2]);
			step = Double.Parse (vals[3]);
			page_inc = Double.Parse (vals[4]);
			page_size = Double.Parse (vals[5]);
			return new GLib.Value (new Gtk.Adjustment (deflt, min, max, step, page_inc, page_size));
		}

		static GLib.Value ParseProperty (ParamSpec pspec, Type propType, string strval)
		{
			IntPtr gtype;
			if (propType != null)
				gtype = ((GLib.GType)propType).Val;
			else if (pspec != null)
				gtype = pspec.ValueType;
			else
				throw new GladeException ("Bad type");

			GLib.TypeFundamentals typef = (GLib.TypeFundamentals)(int)g_type_fundamental (gtype);

			if (gtype == Gtk.Adjustment.GType.Val)
				return ParseAdjustment (strval);
			else if (typef == GLib.TypeFundamentals.TypeEnum)
				return ParseEnum (gtype, strval);
			else if (typef == GLib.TypeFundamentals.TypeFlags)
				return ParseFlags (gtype, strval);
			else
				return ParseBasicType (typef, strval);
		}

		static GLib.Value ParseProperty (Type type, bool childprop, string name, string strval)
		{
			ParamSpec pspec;

			if (childprop)
				pspec = ParamSpec.LookupChildProperty (type, name);
			else
				pspec = ParamSpec.LookupObjectProperty (type, name);
			if (pspec == null)
				throw new GladeException ("Unknown property", type.ToString (), childprop, name, strval);

			try {
				return ParseProperty (pspec, null, strval);
			} catch {
				throw new GladeException ("Could not parse property", type.ToString (), childprop, name, strval);
			}
		}

		static void ParseProperties (Type type, bool childprops, Hashtable props,
					     out string[] propNames, out GLib.Value[] propVals)
		{
			ArrayList names = new ArrayList ();
			ArrayList values = new ArrayList ();

			foreach (string name in props.Keys) {
				string strval = props[name] as string;

				GLib.Value value;
				try {
					value = ParseProperty (type, childprops, name, strval);
					names.Add (name);
					values.Add (value);
				} catch (GladeException ge) {
					Console.Error.WriteLine (ge.Message);
				}
			}

			propNames = (string[])names.ToArray (typeof (string));
			propVals = (GLib.Value[])values.ToArray (typeof (GLib.Value));
		}

		static public void ImportWidget (IStetic stetic, ObjectWrapper wrapper,
						 string className, string id, Hashtable props)
		{
			Hashtable wProps;
			ExtractWrapperProperties (wrapper, props, out wProps);

			IntPtr gtype = g_type_from_name (className);

			string[] propNames;
			GLib.Value[] propVals;
			ParseProperties (ObjectWrapper.WrappedType (wrapper.GetType ()), false, props, out propNames, out propVals);

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
			ParseProperties (widget.GetType (), false, props,
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

			LateImportHelper helper = null;
			foreach (PropertyDescriptor prop in wProps.Keys) {
				if ((prop.GladeFlags & GladeProperty.LateImport) != 0) {
					if (helper == null) {
						helper = new LateImportHelper (stetic, wrapper, false);
						stetic.GladeImportComplete += helper.LateImport;
					}
					helper.Props[prop] = wProps[prop];
				} else
					ParseGladeProperty (wrapper, false, prop, wProps[prop] as string);
			}
		}

		private class LateImportHelper {
			public LateImportHelper (IStetic stetic, ObjectWrapper wrapper, bool childprop) {
				this.stetic = stetic;
				this.wrapper = wrapper;
				this.childprop = childprop;
				Props = new Hashtable ();
			}

			IStetic stetic;
			ObjectWrapper wrapper;
			bool childprop;
			public Hashtable Props;

			public void LateImport () {
				foreach (PropertyDescriptor prop in Props.Keys) {
					ParseGladeProperty (wrapper, childprop,
							    prop, Props[prop] as string);
				}
				stetic.GladeImportComplete -= LateImport;
			}
		}

		static void ParseGladeProperty (ObjectWrapper wrapper, bool childprop,
						PropertyDescriptor prop, string strval)
		{
			try {
				GLib.Value value = ParseProperty (prop.ParamSpec, prop.PropertyType, strval);
				if (prop.PropertyType.IsEnum) {
					GLib.EnumWrapper ewrap = (GLib.EnumWrapper)value;
					prop.SetValue (wrapper, Enum.ToObject (prop.PropertyType, (int)ewrap));
				} else
					prop.SetValue (wrapper, value.Val);
			} catch (Exception e) {
				throw new GladeException ("Could not parse property", wrapper.GetType ().ToString (), childprop, prop.GladeName, strval);
			}
		}

		static public void SetPacking (Gtk.Container parent, Gtk.Widget child, Hashtable childprops)
		{
			string[] propNames;
			GLib.Value[] propVals;
			ParseProperties (parent.GetType (), true, childprops,
					 out propNames, out propVals);

			for (int i = 0; i < propNames.Length; i++)
				parent.ChildSetProperty (child, propNames[i], propVals[i]);
		}

		static string PropToString (ObjectWrapper wrapper, PropertyDescriptor prop)
		{
			object value;

			if ((prop.GladeFlags & GladeProperty.UseUnderlying) != 0) {
				Stetic.Wrapper.Container.ContainerChild ccwrap = wrapper as Stetic.Wrapper.Container.ContainerChild;
				GLib.Value gval;

				if (ccwrap != null) {
					Gtk.Container.ContainerChild cc = (Gtk.Container.ContainerChild)ccwrap.Wrapped;
					gval = new GLib.Value ();
					gtk_container_child_get_property (cc.Parent.Handle, cc.Child.Handle, prop.GladeName, ref gval);
				} else {
					Gtk.Widget widget = wrapper.Wrapped as Gtk.Widget;
					gval = new GLib.Value (widget, prop.GladeName);
					g_object_get_property (widget.Handle, prop.GladeName, ref gval);
				}
				value = gval.Val;
			} else
				value = prop.GetValue (wrapper);
			if (value == null)
				return null;

			// If the property has its default value, we don't need to write it
			if (prop.HasDefault && value.Equals (prop.ParamSpec.Default))
				return null;

			if (value is Gtk.Adjustment) {
				Gtk.Adjustment adj = value as Gtk.Adjustment;
				return String.Format ("{0:G} {1:G} {2:G} {3:G} {4:G} {5:G}",
						      adj.Value, adj.Lower, adj.Upper,
						      adj.StepIncrement, adj.PageIncrement,
						      adj.PageSize);
			} else if (value is Enum && prop.ParamSpec != null) {
				IntPtr klass = g_type_class_ref (prop.ParamSpec.ValueType);

				if (prop.PropertyType.IsDefined (typeof (FlagsAttribute), false)) {
					System.Text.StringBuilder sb = new System.Text.StringBuilder ();
					uint val = (uint)System.Convert.ChangeType (value, typeof (uint));

					while (val != 0) {
						IntPtr flags_value = g_flags_get_first_value (klass, val);
						if (flags_value == IntPtr.Zero)
							break;
						IntPtr fval = Marshal.ReadIntPtr (flags_value);
						val &= ~(uint)fval;

						IntPtr name = Marshal.ReadIntPtr (flags_value, Marshal.SizeOf (typeof (IntPtr)));
						if (name != IntPtr.Zero) {
							if (sb.Length != 0)
								sb.Append ('|');
							sb.Append (GLib.Marshaller.Utf8PtrToString (name));
						}
					}

					g_type_class_unref (klass);
					return sb.ToString ();
				} else {
					int val = (int)System.Convert.ChangeType (value, typeof (int));
					IntPtr enum_value = g_enum_get_value (klass, val);
					g_type_class_unref (klass);

					IntPtr name = Marshal.ReadIntPtr (enum_value, Marshal.SizeOf (typeof (IntPtr)));
					return GLib.Marshaller.Utf8PtrToString (name);
				}
			} else if (value is bool)
				return (bool)value ? "True" : "False";
			else
				return value.ToString ();
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
					if (prop == null)
						continue;
					prop = prop.GladeProperty;
					if (prop.GladeName == null)
						continue;
					if (!prop.VisibleFor (wrapper))
						continue;

					string val = PropToString (wrapper, prop.GladeProperty);
					if (val != null)
						props[prop.GladeName] = val;
				}
			}
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_fundamental (IntPtr gtype);

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_from_name (string name);

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_name (IntPtr gtype);

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_class_ref (IntPtr gtype);

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_class_unref (IntPtr klass);

		[DllImport("glibsharpglue-2")]
		static extern IntPtr gtksharp_object_newv (IntPtr gtype, int n_params, string[] names, GLib.Value[] vals);

		[DllImport("libgtk-win32-2.0-0.dll")]
		static extern void gtk_object_sink (IntPtr raw);

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_get_property (IntPtr obj, string name, ref GLib.Value val);

		[DllImport("libgobject-2.0-0.dll")]
		static extern void g_object_set_property (IntPtr obj, string name, ref GLib.Value val);

		[DllImport("libgtk-win32-2.0-0.dll")]
		static extern void gtk_container_child_get_property (IntPtr parent, IntPtr child, string name, ref GLib.Value val);

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_enum_get_value_by_name (IntPtr enum_class, string name);

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_enum_get_value (IntPtr enum_class, int val);

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_flags_get_value_by_name (IntPtr flags_class, string nick);

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_flags_get_first_value (IntPtr flags_class, uint val);
	}
}
