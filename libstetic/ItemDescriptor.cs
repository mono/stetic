using System;
using System.Collections;

namespace Stetic {

	public abstract class ItemDescriptor {

		Hashtable deps, visdeps;

		// The property's display name
		public abstract string Name { get; }

		// Marks the property as being insensitive when "master"
		// has any of the given values
		public void DisabledIf (ItemDescriptor master, params object[] values)
		{
			if (deps == null)
				deps = new Hashtable ();
			deps[master] = values;
		}

		public bool HasDependencies {
			get {
				return deps != null;
			}
		}

		public bool EnabledFor (ObjectWrapper wrapper)
		{
			if (deps == null)
				return true;

			foreach (PropertyDescriptor dep in deps.Keys) {
				object depValue = dep.GetValue (wrapper);
				foreach (object value in (object[])deps[dep]) {
					if (value.Equals (depValue))
						return false;
				}
			}
			return true;
		}

		// As above, but the property will not even be visible if the
		// master property value is wrong
		public void InvisibleIf (ItemDescriptor master, params object[] values)
		{
			if (visdeps == null)
				visdeps = new Hashtable ();
			visdeps[master] = values;
		}

		public bool VisibleFor (ObjectWrapper wrapper)
		{
			if (visdeps == null)
				return true;

			foreach (PropertyDescriptor dep in visdeps.Keys) {
				object depValue = dep.GetValue (wrapper);
				foreach (object value in (object[])visdeps[dep]) {
					if (value.Equals (depValue))
						return false;
				}
			}
			return true;
		}

		public bool HasVisibility {
			get {
				return visdeps != null;
			}
		}
	}
}
