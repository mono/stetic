using System;
using System.Collections;

namespace Stetic.Wrapper {
	public abstract class Object {

		protected IStetic stetic;
		protected GLib.Object wrapped;

		protected Object (IStetic stetic, GLib.Object obj)
		{
			this.stetic = stetic;
			this.wrapped = obj;

			obj.AddNotification (NotifyHandler);

			// FIXME; cleanup
			wrappers[obj.Handle] = this;
		}

		static Hashtable wrappers = new Hashtable ();

		public static Object Lookup (GLib.Object obj)
		{
			if (obj == null)
				return null;
			else
				return wrappers[obj.Handle] as Stetic.Wrapper.Object;
		}

		void NotifyHandler (object obj, GLib.NotifyArgs args)
		{
			EmitNotify (args.Property);
		}

		public GLib.Object Wrapped {
			get {
				return wrapped;
			}
		}

		public abstract ItemGroup[] ItemGroups { get; }

		public virtual ItemGroup ContextMenuItems {
			get {
				return ItemGroup.Empty;
			}
		}

		public delegate void WrapperNotificationDelegate (Object obj, string propertyName);
		public event WrapperNotificationDelegate Notify;

		protected void EmitNotify (string propertyName)
		{
			if (Notify != null)
				Notify (this, propertyName);
		}
	}
}
