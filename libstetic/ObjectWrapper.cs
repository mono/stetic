using System;
using System.Collections;
using System.Reflection;

namespace Stetic {
	public abstract class ObjectWrapper : IDisposable {

		protected IStetic stetic;
		protected object wrapped;

		protected virtual void Wrap (object obj, bool initialized)
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

		public static void Register (Type type)
		{
			Type wrappedType = WrappedType (type);

			// FIXME: still needed? Why?
			PropertyInfo pinfo = wrappedType.GetProperty ("GType", BindingFlags.Static | BindingFlags.Public);
			if (pinfo != null)
				pinfo.GetValue (null, null);

			if (wrapperTypes.Contains (wrappedType)) {
				Type[] old_type_array = wrapperTypes[wrappedType] as Type[];
				Type[] new_type_array = new Type[old_type_array.Length + 1];
				old_type_array.CopyTo (new_type_array, 0);
				new_type_array[old_type_array.Length] = type;
				wrapperTypes[wrappedType] = new_type_array;
			} else {
				wrapperTypes[wrappedType] = new Type[] { type };
			}

			MethodBase instanceCtor = type.GetMethod ("CreateInstance",
								  BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
								  null, new Type[0], null);
			if (instanceCtor == null)
				instanceCtor = type.GetConstructor (new Type[0]);
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

		public static ObjectWrapper Lookup (object obj)
		{
			if (obj == null)
				return null;
			else
				return wrappers[obj] as Stetic.ObjectWrapper;
		}

		public static Type[] LookupWrapperTypes (object obj)
		{
			if (obj == null)
				return null;
			else 
				return wrapperTypes[obj.GetType ()] as Type[];
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
