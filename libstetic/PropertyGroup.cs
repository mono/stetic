using System;

namespace Stetic {
	public struct PropertyGroup {
		public string Name;
		public PropertyDescriptor[] Properties;

		public PropertyGroup (string name, Type object_type, params string[] propnames) : this (name, null, object_type, propnames) {}

		public PropertyGroup (string name, Type wrapper_type, Type object_type,
				      params string[] propnames)
		{
			Name = name;
			Properties = new PropertyDescriptor[propnames.Length];
			for (int i = 0; i < propnames.Length; i++) {
				Properties[i] = new PropertyDescriptor (wrapper_type, object_type,
									propnames[i]);
			}
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
}
