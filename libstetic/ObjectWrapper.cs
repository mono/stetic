using System;
using System.Collections;

namespace Stetic {
	public abstract class ObjectWrapper : IDisposable {

		protected IStetic stetic;
		protected object wrapped;

		static Hashtable wrappers = new Hashtable ();
		static Hashtable wrapperTypes = new Hashtable ();

		protected ObjectWrapper (IStetic stetic, object obj)
		{
			this.stetic = stetic;
			this.wrapped = obj;

			wrappers[obj] = this;
		}

		public virtual void Dispose ()
		{
			wrappers.Remove (wrapped);
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

		public static void RegisterWrapperType (Type objectWrapper, Type wrappedType)
		{
			if (wrapperTypes.Contains (wrappedType)) {
				Type[] old_type_array = wrapperTypes[wrappedType] as Type[];
				Type[] new_type_array = new Type[old_type_array.Length + 1];
				old_type_array.CopyTo (new_type_array, 0);
				new_type_array[old_type_array.Length] = objectWrapper;
				wrapperTypes[wrappedType] = new_type_array;
			} else {
				wrapperTypes[wrappedType] = new Type[] { objectWrapper };
			}
		}

		public object Wrapped {
			get {
				return wrapped;
			}
		}

		static Hashtable groups = new Hashtable ();
		static Hashtable contextMenus = new Hashtable ();

		protected static void RegisterWrapper (Type t, params ItemGroup[] items)
		{
			groups[t] = items;
		}

		public ItemGroup[] ItemGroups {
			get {
				ItemGroup[] items = (ItemGroup[])groups[GetType ()];
				if (items == null) {
					for (Type t = GetType ().BaseType; t != null; t = t.BaseType) {
						items = (ItemGroup[])groups[t];
						if (items != null) {
							groups[GetType ()] = items;
							break;
						}
					}
				}
				return items;
			}
		}

		protected static void RegisterContextMenu (Type t, ItemGroup group)
		{
			contextMenus[t] = group;
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
