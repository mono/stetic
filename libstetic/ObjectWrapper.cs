using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Stetic {
	public abstract class ObjectWrapper : IDisposable {

		protected IStetic stetic;
		protected object wrapped;

		public virtual void Wrap (object obj, bool initialized)
		{
			this.wrapped = obj;
			wrappers[obj] = this;
		}

		public virtual void Dispose ()
		{
			wrappers.Remove (wrapped);
		}

		static Hashtable wrappers = new Hashtable ();
		static Hashtable wrapperTypes = new Hashtable ();
		static Hashtable factories = new Hashtable ();

		public static Type WrappedType (Type type)
		{
			FieldInfo finfo = type.GetField ("WrappedType", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (finfo != null)
				return finfo.GetValue (null) as Type;
			else {
				Console.WriteLine ("no WrappedType for {0}", type.FullName);
				return null;
			}
		}

		static IntPtr LookupGType (Type gtkSharpType)
		{
			PropertyInfo pinfo = gtkSharpType.GetProperty ("GType", BindingFlags.Static | BindingFlags.Public);
			if (pinfo == null)
				return IntPtr.Zero;

			return ((GLib.GType)pinfo.GetValue (null, null)).Val;
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_name (IntPtr gtype);

		public static string NativeTypeName (Type gtkSharpType)
		{
			IntPtr gtype = LookupGType (gtkSharpType);
			return gtype == IntPtr.Zero ? null : Marshal.PtrToStringAnsi (g_type_name (gtype));
		}

		class ObjectFactory {
			MethodInfo minfo;
			ConstructorInfo cinfo;
			IntPtr gtype;

			public ObjectFactory (Type wrapperType, Type wrappedType)
			{
				minfo = wrapperType.GetMethod ("CreateInstance",
							       BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
							       null, new Type[0], null);
				if (minfo != null)
					return;

				cinfo = wrappedType.GetConstructor (new Type[0]);
				if (cinfo != null)
					return;

				cinfo = wrappedType.GetConstructor (new Type[] { typeof (IntPtr) });
				gtype = LookupGType (wrappedType);
			}

			[DllImport("libgobject-2.0-0.dll")]
			static extern IntPtr g_object_new (IntPtr gtype, IntPtr dummy);

			public object New ()
			{
				if (minfo != null)
					return minfo.Invoke (null, new object[0]);
				else if (gtype == IntPtr.Zero)
					return cinfo.Invoke (null, new object[0]);
				else {
					IntPtr raw = g_object_new (gtype, IntPtr.Zero);
					return cinfo.Invoke (new object[] { raw });
				}
			}
		}

		public static void Register (Type type)
		{
			Type wrappedType = WrappedType (type);
			string className = NativeTypeName (wrappedType);
			if (className != null)
				wrapperTypes[className] = type;

			factories[type] = new ObjectFactory (type, wrappedType);

			// Run wrapper registration methods
			Type regType = type;
			while (type != typeof (ObjectWrapper)) {
				MethodInfo register = type.GetMethod ("Register",
								      BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
								      null, new Type[] { typeof (Type) }, null);
				if (register != null)
					register.Invoke (null, new object[] { regType });
				type = type.BaseType;
			}
		}

		public static ObjectWrapper Create (IStetic stetic, Type type)
		{
			ObjectWrapper wrapper = Activator.CreateInstance (type) as ObjectWrapper;
			if (wrapper == null)
				return null;
			wrapper.stetic = stetic;

			ObjectFactory factory = factories[type] as ObjectFactory;
			wrapper.Wrap (factory.New (), false);
			return wrapper;
		}

		public static ObjectWrapper Create (IStetic stetic, Type type, object wrapped)
		{
			ObjectWrapper wrapper = Activator.CreateInstance (type) as ObjectWrapper;
			if (wrapper == null)
				return null;
			wrapper.stetic = stetic;

			wrapper.Wrap (wrapped, true);
			return wrapper;
		}

		public static ObjectWrapper Create (IStetic stetic, string className)
		{
			Type type = wrapperTypes[className] as Type;
			if (type == null)
				return null;

			ObjectWrapper wrapper = Activator.CreateInstance (type) as ObjectWrapper;
			if (wrapper == null)
				return null;
			wrapper.stetic = stetic;
			return wrapper;
		}

		public static ObjectWrapper GladeImport (IStetic stetic, string className, string id, Hashtable props)
		{
			Type type = (Type)wrapperTypes[className];
			if (type == null)
				throw new GladeException ("No Stetic wrapper for type", className);

			MethodInfo info = type.GetMethod ("GladeImport", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (info == null)
				throw new GladeException ("No Import method for type " + type.FullName, className);

			ObjectWrapper wrapper = Activator.CreateInstance (type) as ObjectWrapper;
			if (wrapper == null)
				throw new GladeException ("Can't create wrapper for type " + type.FullName, className);
			wrapper.stetic = stetic;
			try {
				info.Invoke (wrapper, new object[] { className, id, props });
			} catch (TargetInvocationException tie) {
				throw tie.InnerException;
			}
			return wrapper;
		}

		public static ObjectWrapper Lookup (object obj)
		{
			if (obj == null)
				return null;
			else
				return wrappers[obj] as Stetic.ObjectWrapper;
		}

		public object Wrapped {
			get {
				return wrapped;
			}
		}

		static Hashtable groups = new Hashtable ();
		static Hashtable contextMenus = new Hashtable ();

		protected static ItemGroup AddItemGroup (Type type, string name, params string[] items)
		{
			ArrayList list = groups[type] as ArrayList;
			if (list == null)
				groups[type] = list = new ArrayList ();
			ItemGroup group = new ItemGroup (name, type, WrappedType (type), items);
			list.Add (group);
			return group;
		}

		public ArrayList ItemGroups {
			get {
				return groups[GetType ()] as ArrayList;
			}
		}

		protected static ArrayList GetItemGroups (Type type)
		{
			return groups[type] as ArrayList;
		}

		protected static ItemGroup AddContextMenuItems (Type type, params string[] items)
		{
			ItemGroup group = new ItemGroup (null, type, WrappedType (type), items);
			contextMenus[type] = group;
			return group;
		}

		public ItemGroup ContextMenuItems {
			get {
				if (contextMenus.Contains (GetType ()))
					return (ItemGroup)contextMenus[GetType ()];
				else
					return ItemGroup.Empty;
			}
		}

		public delegate void WrapperNotificationDelegate (object obj, string propertyName);
		public event WrapperNotificationDelegate Notify;

		protected virtual void EmitNotify (string propertyName)
		{
			if (Notify != null)
				Notify (this, propertyName);
		}
	}
}
