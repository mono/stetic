using System;

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
}
