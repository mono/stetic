using System;
using System.Collections;

namespace Stetic {

	public abstract class ItemDescriptor {

		Hashtable deps, visdeps;

		// The property's display name
		public abstract string Name { get; }

		// Marks the property as depending on master (which
		// must have Type bool). The property will be
		// sensitive in the PropertyGrid when master is true.
		public void DependsOn (ItemDescriptor master)
		{
			if (deps == null)
				deps = new Hashtable ();
			deps[master] = true;
		}

		// Marks the property as depending inversely on master
		// (which must have Type bool). The property will be
		// sensitive in the PropertyGrid when master is false.
		public void DependsInverselyOn (ItemDescriptor master)
		{
			if (deps == null)
				deps = new Hashtable ();
			deps[master] = false;
		}

		public bool HasDependencies {
			get {
				return deps != null;
			}
		}

		public bool EnabledFor (ObjectWrapper wrapper)
		{
			if (deps != null) {
				foreach (PropertyDescriptor dep in deps.Keys) {
					if ((bool)dep.GetValue (wrapper) != (bool)deps[dep])
						return false;
				}
			}
			return true;
		}

		// As above, but the property will not even be visible if the
		// master property is false.
		public void VisibleIf (ItemDescriptor master)
		{
			if (visdeps == null)
				visdeps = new Hashtable ();
			visdeps[master] = true;
		}

		public void InvisibleIf (ItemDescriptor master)
		{
			if (visdeps == null)
				visdeps = new Hashtable ();
			visdeps[master] = false;
		}

		public bool VisibleFor (ObjectWrapper wrapper)
		{
			if (visdeps != null) {
				foreach (PropertyDescriptor dep in visdeps.Keys) {
					if ((bool)dep.GetValue (wrapper) != (bool)visdeps[dep])
						return false;
				}
			}
			return true;
		}
	}
}
