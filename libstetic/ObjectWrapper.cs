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
		static Hashtable instanceCtors = new Hashtable ();

		static Type WrappedType (Type type)
		{
			FieldInfo finfo = type.GetField ("WrappedType", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (finfo != null)
				return finfo.GetValue (null) as Type;
			else {
				Console.WriteLine ("no WrappedType for {0}", type.FullName);
				return null;
			}
		}

		[DllImport("libgobject-2.0-0.dll")]
		static extern IntPtr g_type_name (IntPtr gtype);

		static string NativeTypeName (Type gtkSharpType)
		{
			PropertyInfo pinfo = gtkSharpType.GetProperty ("GType", BindingFlags.Static | BindingFlags.Public);
			if (pinfo == null)
				return null;

			GLib.GType gtype = (GLib.GType)pinfo.GetValue (null, null);
			return Marshal.PtrToStringAnsi (g_type_name (gtype.Val));
		}

		public static void Register (Type type)
		{
			Type wrappedType = WrappedType (type);
			string className = NativeTypeName (wrappedType);
			if (className != null)
				wrapperTypes[className] = type;

			MethodBase instanceCtor = type.GetMethod ("CreateInstance",
								  BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
								  null, new Type[0], null);
			if (instanceCtor == null)
				instanceCtor = wrappedType.GetConstructor (new Type[0]);
			instanceCtors[type] = instanceCtor;

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

			MethodBase instanceCtor = instanceCtors[type] as MethodBase;
			wrapper.Wrap (instanceCtor.Invoke (null, new object[0]), false);
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

		public static ObjectWrapper Create (IStetic stetic, string className, object wrapped)
		{
			Type type = wrapperTypes[className] as Type;
			if (type == null)
				return null;
			return Create (stetic, type, wrapped);
		}

		public static ObjectWrapper GladeImport (IStetic stetic, string className, string id, Hashtable props)
		{
			Type type = (Type)wrapperTypes[className];
			if (type == null)
				return null;

			MethodInfo info = type.GetMethod ("GladeImport", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (info == null)
				return null;

			ObjectWrapper wrapper = Activator.CreateInstance (type) as ObjectWrapper;
			if (wrapper == null)
				return null;
			wrapper.stetic = stetic;
			info.Invoke (wrapper, new object[] { className, id, props });
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
