using System;

namespace Stetic {
	public interface IObjectWrapper {
		PropertyGroup[] PropertyGroups { get; }

		GLib.Object Wrapped { get; }
	}
}
