using System;
using System.Collections;

namespace Stetic {

	public abstract class ItemDescriptor {

		ArrayList dependencies = new ArrayList (), inverseDependencies = new ArrayList ();

		// The property's display name
		public abstract string Name { get; }

		// Marks the property as depending on master (which
		// must have Type bool). The property will be
		// sensitive in the PropertyGrid when master is true.
		public void DependsOn (ItemDescriptor master)
		{
			dependencies.Add (master);
		}

		// Marks the property as depending inversely on master
		// (which must have Type bool). The property will be
		// sensitive in the PropertyGrid when master is false.
		public void DependsInverselyOn (ItemDescriptor master)
		{
			inverseDependencies.Add (master);
		}

		public bool HasDependencies {
			get {
				return dependencies.Count > 0 || inverseDependencies.Count > 0;
			}
		}

		public bool EnabledFor (object obj)
		{
			foreach (PropertyDescriptor dep in dependencies) {
				if (!(bool)dep.GetValue (obj))
					return false;
			}
			foreach (PropertyDescriptor dep in inverseDependencies) {
				if ((bool)dep.GetValue (obj))
					return false;
			}
			return true;
		}
	}
}
