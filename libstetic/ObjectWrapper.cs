using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

namespace Stetic {
	public abstract class ObjectWrapper : IDisposable {

		protected IProject proj;
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

		public static ObjectWrapper Create (IProject proj, object wrapped)
		{
			return Create (proj, wrapped, true);
		}

		public static ObjectWrapper Create (IProject proj, object wrapped, bool initialized)
		{
			ClassDescriptor klass = Registry.LookupClass (wrapped.GetType ());
			if (klass == null)
				throw new ApplicationException ("No wrapper for " + wrapped.GetType ().FullName);
			ObjectWrapper wrapper = Activator.CreateInstance (klass.WrapperType) as ObjectWrapper;
			if (wrapper == null)
				return null;
			wrapper.proj = proj;

			wrapper.Wrap (wrapped, initialized);
			return wrapper;
		}

		public static ObjectWrapper GladeImport (IProject proj, XmlElement elem)
		{
			string className = elem.GetAttribute ("class");
			ClassDescriptor klass = Registry.LookupClass (className);
			if (klass == null)
				throw new GladeException ("No Stetic wrapper for type", className);

			MethodInfo info = klass.WrapperType.GetMethod ("GladeImport", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (info == null)
				throw new GladeException ("No Import method for type " + klass.WrapperType.FullName, className);

			ObjectWrapper wrapper = Activator.CreateInstance (klass.WrapperType) as ObjectWrapper;
			if (wrapper == null)
				throw new GladeException ("Can't create wrapper for type " + klass.WrapperType.FullName, className);
			wrapper.proj = proj;
			try {
				info.Invoke (wrapper, new object[] { elem });
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

		public delegate void WrapperNotificationDelegate (object obj, string propertyName);
		public event WrapperNotificationDelegate Notify;

		protected virtual void EmitNotify (string propertyName)
		{
			if (Notify != null)
				Notify (this, propertyName);
		}
	}
}
