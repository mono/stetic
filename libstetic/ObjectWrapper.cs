using System;
using System.Collections;

namespace Stetic {
	public abstract class ObjectWrapper : IDisposable {

		protected IStetic stetic;
		protected object wrapped;

		static Hashtable wrappers = new Hashtable ();

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

		public object Wrapped {
			get {
				return wrapped;
			}
		}

		static Hashtable groups = new Hashtable ();
		static Hashtable contextMenus = new Hashtable ();

		protected static void RegisterItems (Type t, params ItemGroup[] items)
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

		protected void EmitNotify (string propertyName)
		{
			if (Notify != null)
				Notify (this, propertyName);
		}
	}
}
