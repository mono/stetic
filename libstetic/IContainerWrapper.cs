using System;
using System.Collections;

namespace Stetic {
	public interface IContainerWrapper : IWidgetWrapper {
		PropertyGroup[] ChildPropertyGroups { get; }

		bool HExpandable { get; }
		bool VExpandable { get; }

		event ContentsChangedHandler ContentsChanged;
	}

	public delegate void ContentsChangedHandler (IContainerWrapper container);
}
