using System;

namespace Stetic {
	public interface IContainerWrapper : IObjectWrapper {
		PropertyGroup[] ChildPropertyGroups { get; }

		bool HExpandable { get; }
		bool VExpandable { get; }
		event ExpandabilityChangedHandler ExpandabilityChanged;
	}

	public delegate void ExpandabilityChangedHandler (IContainerWrapper container);
}
