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

		public PropertyGroup (string name, Type type, params string[] propnames)
		{
			Name = name;
			Properties = new PropertyDescriptor[propnames.Length];
			for (int i = 0; i < propnames.Length; i++)
				Properties[i] = new PropertyDescriptor (type, propnames[i]);
		}

		public PropertyDescriptor this [string propname] {
			get {
				foreach (PropertyDescriptor prop in Properties) {
					if (prop.Name == propname)
						return prop;
				}
				return null;
			}
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
		IEnumerable InsensitiveProperties { get; }

		event SensitivityChangedDelegate SensitivityChanged;
	}
}
