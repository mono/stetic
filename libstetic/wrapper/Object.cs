using System;
using System.Collections;

namespace Stetic.Wrapper {
	public abstract class Object : Stetic.ObjectWrapper {

		public override void Wrap (object obj, bool initialized)
		{
			base.Wrap (obj, initialized);
			((GLib.Object)Wrapped).AddNotification (NotifyHandler);

			// FIXME; arrange for wrapper disposal?
		}

		public override void Dispose ()
		{
			((GLib.Object)Wrapped).RemoveNotification (NotifyHandler);
			base.Dispose ();
		}

		public static Object Lookup (GLib.Object obj)
		{
			return Stetic.ObjectWrapper.Lookup (obj) as Stetic.Wrapper.Object;
		}

		void NotifyHandler (object obj, GLib.NotifyArgs args)
		{
			// Translate gtk names into descriptor names.
			
			foreach (ItemGroup group in ClassDescriptor.ItemGroups) {
				foreach (ItemDescriptor item in group) {
					TypedPropertyDescriptor prop = item as TypedPropertyDescriptor;
					if (prop != null && prop.GladeName == args.Property) {
						EmitNotify (prop.Name);
						return;
					}
				}
			}
		}
	}
}
