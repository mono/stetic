using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

namespace Stetic {

	public enum FileFormat {
		Native,
		Glade
	}

	public abstract class ObjectWrapper : IDisposable {

		protected IProject proj;
		protected object wrapped;
		protected ClassDescriptor classDescriptor;
		SignalCollection signals;

		public SignalCollection Signals {
			get {
				if (signals == null)
					signals = new SignalCollection (this);
				return signals;
			}
		}
		
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
			ClassDescriptor klass = Registry.LookupClassByName (wrapped.GetType ().FullName);
			ObjectWrapper wrapper = klass.CreateWrapper ();
			wrapper.proj = proj;
			wrapper.classDescriptor = klass;
			wrapper.Wrap (wrapped, true);
			return wrapper;
		}

		internal static void Bind (IProject proj, ClassDescriptor klass, ObjectWrapper wrapper, object wrapped, bool initialized)
		{
			wrapper.proj = proj;
			wrapper.classDescriptor = klass;
			wrapper.Wrap (wrapped, initialized);
		}
		
		public virtual void Read (XmlElement elem, FileFormat format)
		{
			throw new System.NotSupportedException ();
		}
		
		public virtual XmlElement Write (XmlDocument doc, FileFormat format)
		{
			throw new System.NotSupportedException ();
		}

		public static ObjectWrapper Read (IProject proj, XmlElement elem, FileFormat format)
		{
			string className = elem.GetAttribute ("class");
			ClassDescriptor klass;
			if (format == FileFormat.Native)
				klass = Registry.LookupClassByName (className);
			else
				klass = Registry.LookupClassByCName (className);
			
			if (klass == null) {
				ErrorWidget we = new ErrorWidget (className);
				ErrorWidgetWrapper wrap = (ErrorWidgetWrapper) Create (proj, we);
				wrap.Read (elem, format);
				return wrap;
			}

			ObjectWrapper wrapper = klass.CreateWrapper ();
			wrapper.proj = proj;
			try {
				wrapper.Read (elem, format);
			} catch (Exception ex) {
				ErrorWidget we = new ErrorWidget (ex);
				ErrorWidgetWrapper wrap = (ErrorWidgetWrapper) Create (proj, we);
				wrap.Read (elem, format);
				Console.WriteLine (ex);
				return wrap;
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

		public IProject Project {
			get {
				return proj;
			}
		}

		public ClassDescriptor ClassDescriptor {
			get { return classDescriptor; }
		}
		
		public void NotifyChanged ()
		{
			OnObjectChanged (new ObjectWrapperEventArgs (this));
		}
		
		public delegate void WrapperNotificationDelegate (object obj, string propertyName);
		
		public event WrapperNotificationDelegate Notify;
		public event SignalEventHandler SignalAdded;
		public event SignalEventHandler SignalRemoved;
		public event SignalChangedEventHandler SignalChanged;
		
		// Fired when any information of the object changes.
		public event ObjectWrapperEventHandler ObjectChanged;

		internal protected virtual void OnObjectChanged (ObjectWrapperEventArgs args)
		{
			if (ObjectChanged != null)
				ObjectChanged (this, args);
		}
		
		protected virtual void EmitNotify (string propertyName)
		{
			if (Notify != null)
				Notify (this, propertyName);
		}

		internal protected virtual void OnSignalAdded (SignalEventArgs args)
		{
			OnObjectChanged (args);
			if (SignalAdded != null)
				SignalAdded (this, args);
		}
		
		internal protected virtual void OnSignalRemoved (SignalEventArgs args)
		{
			OnObjectChanged (args);
			if (SignalRemoved != null)
				SignalRemoved (this, args);
		}
		
		internal protected virtual void OnSignalChanged (SignalChangedEventArgs args)
		{
			OnObjectChanged (args);
			if (SignalChanged != null)
				SignalChanged (this, args);
		}
	}
}
