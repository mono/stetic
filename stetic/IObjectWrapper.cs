using Gtk;
using Gdk;
using GLib;
using System;
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

	public delegate void SensitivityChangedDelegate (string property, bool sensitivity);

	public interface IObjectWrapper {
		PropertyGroup[] PropertyGroups { get; }

//		event SensitivityChangedDelegate SensitivityChanged;
	}

}
