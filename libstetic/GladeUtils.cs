using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Xml;

namespace Stetic {

	public static class GladeUtils {

		static object GetProperty (XmlElement elem, string selector, object defaultValue, bool extract)
		{
			XmlElement prop = (XmlElement)elem.SelectSingleNode (selector);
			if (prop == null)
				return defaultValue;
			if (extract)
				prop.ParentNode.RemoveChild (prop);
			return ParseProperty (null, defaultValue.GetType (), prop.InnerText).Val;
		}

		public static object GetProperty (XmlElement elem, string name, object defaultValue)
		{
			return GetProperty (elem, "./property[@name='" + name + "']", defaultValue, false);
		}

		public static object ExtractProperty (XmlElement elem, string name, object defaultValue)
		{
			return GetProperty (elem, "./property[@name='" + name + "']", defaultValue, true);
		}

		public static object GetChildProperty (XmlElement elem, string name, object defaultValue)
		{
			return GetProperty (elem, "./packing/property[@name='" + name + "']", defaultValue, false);
		}

		public static object ExtractChildProperty (XmlElement elem, string name, object defaultValue)
		{
			return GetProperty (elem, "./packing/property[@name='" + name + "']", defaultValue, true);
		}

		public static void SetProperty (XmlElement elem, string name, string value)
		{
			XmlElement prop_elem = elem.OwnerDocument.CreateElement ("property");
			prop_elem.SetAttribute ("name", name);
			prop_elem.InnerText = value;
			elem.AppendChild (prop_elem);
		}

		public static void SetChildProperty (XmlElement elem, string name, string value)
		{
			XmlElement packing_elem = elem["packing"];
			if (packing_elem == null) {
				packing_elem = elem.OwnerDocument.CreateElement ("packing");
				elem.AppendChild (packing_elem);
			}
			SetProperty (packing_elem, name, value);
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
				foreach (string flag in strval.Split ('|')) {
					if (flag == "")
						continue;
					IntPtr flags_value = g_flags_get_value_by_name (flags_class, flag);
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

		static GLib.Value ParseUnichar (string strval)
		{
			return new GLib.Value (strval.Length == 1 ? (uint)strval[0] : 0U);
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
			else if (pspec != null && pspec.IsUnichar)
				return ParseUnichar (strval);
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

		static void ParseProperties (Type type, bool childprops, IEnumerable props,
					     out string[] propNames, out GLib.Value[] propVals)
		{
			ArrayList names = new ArrayList ();
			ArrayList values = new ArrayList ();

			foreach (XmlElement prop in props) {
				string name = prop.GetAttribute ("name");
				string strval = prop.InnerText;

				// Skip translation context
				if (prop.GetAttribute ("context") == "yes" &&
				    strval.IndexOf ('|') != -1)
					strval = strval.Substring (strval.IndexOf ('|') + 1);

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

		static void ExtractProperties (ClassDescriptor klass, XmlElement elem,
					       out Hashtable rawProps, out Hashtable overrideProps)
		{
			rawProps = new Hashtable ();
			overrideProps = new Hashtable ();
			foreach (ItemGroup group in klass.ItemGroups) {
				foreach (ItemDescriptor item in group) {
					PropertyDescriptor prop = item as PropertyDescriptor;
					if (prop == null)
						continue;
					prop = prop.GladeProperty;
					if (prop.GladeName == null)
						continue;

					XmlNode prop_node = elem.SelectSingleNode ("property[@name='" + prop.GladeName + "']");
					if (prop_node == null)
						continue;

					if (prop.GladeOverride)
						overrideProps[prop] = prop_node;
					else
						rawProps[prop] = prop_node;
				}
			}
		}

		static public void ImportWidget (ObjectWrapper wrapper, XmlElement elem)
		{
			string className = elem.GetAttribute ("class");
			if (className == null)
				throw new GladeException ("<widget> node with no class name");

			ClassDescriptor klass = Registry.LookupClass (className);
			if (klass == null)
				throw new GladeException ("No stetic ClassDescriptor for " + className);

			Hashtable rawProps, overrideProps;
			ExtractProperties (klass, elem, out rawProps, out overrideProps);

			string[] propNames;
			GLib.Value[] propVals;
			ParseProperties (klass.WrappedType, false, rawProps.Values,
					 out propNames, out propVals);

			Gtk.Widget widget;

			if (wrapper.Wrapped == null) {
				IntPtr raw = gtksharp_object_newv (klass.GType.Val, propNames.Length, propNames, propVals);
				if (raw == IntPtr.Zero)
					throw new GladeException ("Could not create widget", className);

				widget = (Gtk.Widget)GLib.Object.GetObject (raw, true);
				if (widget == null) {
					gtk_object_sink (raw);
					throw new GladeException ("Could not create gtk# wrapper", className);
				}
				wrapper.Wrap (widget, true);
			} else {
				widget = (Gtk.Widget)wrapper.Wrapped;
				for (int i = 0; i < propNames.Length; i++)
					g_object_set_property (widget.Handle, propNames[i], ref propVals[i]);
			}
			MarkTranslatables (widget, rawProps);

			widget.Name = elem.GetAttribute ("id");

			SetOverrideProperties (wrapper, overrideProps);
			MarkTranslatables (widget, overrideProps);
		}

		static void SetOverrideProperties (ObjectWrapper wrapper, Hashtable overrideProps)
		{
			foreach (PropertyDescriptor prop in overrideProps.Keys) {
				XmlElement prop_elem = overrideProps[prop] as XmlElement;

				try {
					GLib.Value value = ParseProperty (prop.ParamSpec, prop.PropertyType, prop_elem.InnerText);
					if (prop.PropertyType.IsEnum) {
						GLib.EnumWrapper ewrap = (GLib.EnumWrapper)value;
						prop.SetValue (wrapper.Wrapped, Enum.ToObject (prop.PropertyType, (int)ewrap));
					} else
						prop.SetValue (wrapper.Wrapped, value.Val);
				} catch (Exception e) {
					throw new GladeException ("Could not parse property", wrapper.GetType ().ToString (), wrapper is Stetic.Wrapper.Container.ContainerChild, prop.GladeName, prop_elem.InnerText);
				}
			}
		}

		static void MarkTranslatables (object obj, Hashtable props)
		{
			foreach (PropertyDescriptor prop in props.Keys) {
				if (!prop.Translatable)
					continue;

				XmlElement prop_elem = props[prop] as XmlElement;
				if (prop_elem.GetAttribute ("translatable") != "yes") {
					prop.SetTranslated (obj, false);
					continue;
				}

				prop.SetTranslated (obj, true);
				if (prop_elem.GetAttribute ("context") == "yes") {
					string strval = prop_elem.InnerText;
					int bar = strval.IndexOf ('|');
					if (bar != -1)
						prop.SetTranslationContext (obj, strval.Substring (0, bar));
				}

				if (prop_elem.HasAttribute ("comments"))
					prop.SetTranslationComment (obj, prop_elem.GetAttribute ("comments"));
			}
		}

		static public void SetPacking (Stetic.Wrapper.Container.ContainerChild wrapper, XmlElement child_elem)
		{
			Gtk.Container.ContainerChild cc = wrapper.Wrapped as Gtk.Container.ContainerChild;

			ClassDescriptor klass = Registry.LookupClass (cc.GetType ());
			if (klass == null)
				throw new GladeException ("No stetic ClassDescriptor for " + cc.GetType ().FullName);

			Hashtable rawProps, overrideProps;
			ExtractProperties (klass, child_elem["packing"], out rawProps, out overrideProps);

			string[] propNames;
			GLib.Value[] propVals;
			ParseProperties (cc.Parent.GetType (), true, rawProps.Values,
					 out propNames, out propVals);

			for (int i = 0; i < propNames.Length; i++)
				cc.Parent.ChildSetProperty (cc.Child, propNames[i], propVals[i]);
			MarkTranslatables (cc, rawProps);

			SetOverrideProperties (wrapper, overrideProps);
			MarkTranslatables (cc, overrideProps);
		}

		static string PropToString (ObjectWrapper wrapper, PropertyDescriptor prop)
		{
			object value;

			if (!prop.GladeOverride) {
				Stetic.Wrapper.Container.ContainerChild ccwrap = wrapper as Stetic.Wrapper.Container.ContainerChild;
				GLib.Value gval;

				if (ccwrap != null) {
					Gtk.Container.ContainerChild cc = (Gtk.Container.ContainerChild)ccwrap.Wrapped;
					gval = new GLib.Value (new GLib.GType (prop.ParamSpec.ValueType));
					gtk_container_child_get_property (cc.Parent.Handle, cc.Child.Handle, prop.GladeName, ref gval);
				} else {
					Gtk.Widget widget = wrapper.Wrapped as Gtk.Widget;
					gval = new GLib.Value (widget, prop.GladeName);
					g_object_get_property (widget.Handle, prop.GladeName, ref gval);
				}
				value = gval.Val;
			} else
				value = prop.GetValue (wrapper.Wrapped);
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

		static public XmlElement ExportWidget (ObjectWrapper wrapper, XmlDocument doc)
		{
			Type wrappedType = wrapper.Wrapped.GetType ();
			ClassDescriptor klass = Registry.LookupClass (wrappedType);

			XmlElement  elem = doc.CreateElement ("widget");
			elem.SetAttribute ("class", klass.CName);
			elem.SetAttribute ("id", ((Gtk.Widget)wrapper.Wrapped).Name);

			GetProps (wrapper, elem);
			return elem;
		}

		static public void GetProps (ObjectWrapper wrapper, XmlElement parent_elem)
		{
			ClassDescriptor klass = Registry.LookupClass (wrapper.Wrapped.GetType ());

			foreach (ItemGroup group in klass.ItemGroups) {
				foreach (ItemDescriptor item in group) {
					PropertyDescriptor prop = item as PropertyDescriptor;
					if (prop == null)
						continue;
					prop = prop.GladeProperty;
					if (prop.GladeName == null)
						continue;
					if (!prop.VisibleFor (wrapper.Wrapped))
						continue;

					string val = PropToString (wrapper, prop);
					if (val == null)
						continue;

					XmlElement prop_elem = parent_elem.OwnerDocument.CreateElement ("property");
					prop_elem.SetAttribute ("name", prop.GladeName);
					prop_elem.InnerText = val;

					if (prop.Translatable && prop.IsTranslated (wrapper.Wrapped)) {
						prop_elem.SetAttribute ("translatable", "yes");
						if (prop.TranslationContext (wrapper.Wrapped) != null) {
							prop_elem.SetAttribute ("context", "yes");
							prop_elem.InnerText = prop.TranslationContext (wrapper.Wrapped) + "|" + prop_elem.InnerText;
						}
						if (prop.TranslationComment (wrapper.Wrapped) != null)
							prop_elem.SetAttribute ("comments", prop.TranslationComment (wrapper.Wrapped));
					}

					parent_elem.AppendChild (prop_elem);
				}
			}
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_fundamental (IntPtr gtype);

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
