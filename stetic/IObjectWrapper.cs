using Gtk;
using Gdk;
using GLib;
using System;
using System.Collections;
using System.Reflection;

namespace Stetic {
	public struct PropertyGroup {
		public string Name;
		public PropertyDescriptor[] Properties;

		public PropertyGroup (string name, PropertyDescriptor[] properties)
		{
			Name = name;
			Properties = properties;
		}
	}

	public interface IObjectWrapper {
		PropertyGroup[] PropertyGroups { get; }
	}


	public delegate void ExpandabilityChangedHandler (IContainerWrapper container);

	public interface IContainerWrapper : IObjectWrapper {
		PropertyGroup[] ChildPropertyGroups { get; }

		bool HExpandable { get; }
		bool VExpandable { get; }
		event ExpandabilityChangedHandler ExpandabilityChanged;
	}


	public delegate void SensitivityChangedDelegate (string property, bool sensitivity);

	public interface IPropertySensitizer {
		IEnumerable InsensitiveProperties ();

		event SensitivityChangedDelegate SensitivityChanged;
	}
}
