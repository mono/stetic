using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

namespace Stetic
{
	public class TypedClassDescriptor: ClassDescriptor 
	{
		Type wrapped, wrapper;
		GLib.GType gtype;

		MethodInfo ctorMethodInfo;
		MethodInfo ctorMethodInfoWithClass;
		ConstructorInfo cinfo;
		bool useGTypeCtor;
		Gdk.Pixbuf icon;
		
		const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		
		static Gdk.Pixbuf missingIcon;

		public TypedClassDescriptor (Assembly assembly, XmlElement elem)
		{
			wrapped = Registry.GetType (elem.GetAttribute ("type"), true);
			if (elem.HasAttribute ("wrapper"))
			    wrapper = Registry.GetType (elem.GetAttribute ("wrapper"), true);
			else {
				for (Type type = wrapped.BaseType; type != null; type = type.BaseType) {
					TypedClassDescriptor parent = Registry.LookupClassByName (type.FullName) as TypedClassDescriptor;
					if (parent != null) {
						wrapper = parent.WrapperType;
						break;
					}
				}
				if (wrapper == null)
					throw new ArgumentException (string.Format ("No wrapper type for class {0}", wrapped.FullName));
			}

			gtype = (GLib.GType)wrapped;
			cname = gtype.ToString ();

			string iconname = elem.GetAttribute ("icon");
			if (iconname.Length > 0) {
				try {
					// Using the pixbuf resource constructor generates a gdk warning.
					Gdk.PixbufLoader loader = new Gdk.PixbufLoader (assembly, iconname);
					icon = loader.Pixbuf;
				} catch {
					icon = GetDefaultIcon ();
				}
			} else
				icon = GetDefaultIcon ();
			
			BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
						

			ctorMethodInfoWithClass = wrapper.GetMethod ("CreateInstance", flags, null, new Type[] { typeof(ClassDescriptor)}, null);
			if (ctorMethodInfoWithClass == null) {
				ctorMethodInfo = wrapper.GetMethod ("CreateInstance", flags, null, Type.EmptyTypes, null);
			}
			
			// Look for a constructor even if a CreateInstance method was
			// found, since it may return null.
			cinfo = wrapped.GetConstructor (Type.EmptyTypes);
			if (cinfo == null) {
				useGTypeCtor = true;
				cinfo = wrapped.GetConstructor (new Type[] { typeof (IntPtr) });
			}
			Load (elem);
		}
		
		public override Gdk.Pixbuf Icon {
			get {
				return icon;
			}
		}

		public override string WrappedTypeName {
			get { return WrappedType.FullName; }
		}
		
		public Type WrapperType {
			get {
				return wrapper;
			}
		}
		
		public Type WrappedType {
			get {
				return wrapped;
			}
		}

		public GLib.GType GType {
			get {
				return gtype;
			}
		}
		
		public override ObjectWrapper CreateWrapper ()
		{
			return (ObjectWrapper) Activator.CreateInstance (WrapperType);
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_object_new (IntPtr gtype, IntPtr dummy);

		public override object CreateInstance (IProject proj)
		{
			object inst;

			if (ctorMethodInfoWithClass != null) {
				inst = ctorMethodInfoWithClass.Invoke (null, new object[] { this });
				if (inst != null) return inst;
			}
			if (ctorMethodInfo != null) {
				inst = ctorMethodInfo.Invoke (null, new object[0]);
				if (inst != null) return inst;
			}
			if (!useGTypeCtor)
				inst = cinfo.Invoke (null, new object[0]);
			else {
				IntPtr raw = g_object_new (gtype.Val, IntPtr.Zero);
				inst = cinfo.Invoke (new object[] { raw });
			}

			return inst;
		}
		
		internal protected override ItemDescriptor CreateItemDescriptor (XmlElement elem, ItemGroup group)
		{
			if (elem.Name == "property")
				return new TypedPropertyDescriptor (elem, group, this);
			else if (elem.Name == "signal")
				return new TypedSignalDescriptor (elem, group, this);
			else
				return base.CreateItemDescriptor (elem, group);
		}
		
		Gdk.Pixbuf GetDefaultIcon ()
		{
			if (missingIcon == null)
				missingIcon = Gtk.IconTheme.Default.LoadIcon (Gtk.Stock.MissingImage, 16, 0);
			return missingIcon;
		}
	}
}
