using System;
using System.Reflection;

namespace Stetic {
	public struct ItemGroup {
		public string Name;
		public ItemDescriptor[] Items;

		public ItemGroup (string name, Type objectType, params string[] names) : this (name, null, objectType, names) {}


		const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		public ItemGroup (string name, Type wrapperType, Type objectType, params string[] names)
		{
			Name = name;
			Items = new ItemDescriptor[names.Length];
			for (int i = 0; i < names.Length; i++) {
				if (wrapperType == null || names[i].IndexOf ('.') != -1)
					Items[i] = new PropertyDescriptor (objectType, names[i]);
				else if (wrapperType.GetProperty (names[i], flags) != null || objectType.GetProperty (names[i], flags) != null)
					Items[i] = new PropertyDescriptor (wrapperType, objectType, names[i]);
				else if (wrapperType.GetMethod (names[i], flags, null, new Type[0], null) != null)
					Items[i] = new CommandDescriptor (wrapperType, names[i]);
				else if (wrapperType.GetMethod (names[i], flags, null, new Type[] { typeof (Stetic.IWidgetSite) }, null) != null)
					Items[i] = new CommandDescriptor (wrapperType, names[i]);
				else
					throw new ApplicationException ("Bad item name " + names[i] + " in " + wrapperType.Name);
			}
		}

		public ItemDescriptor this [string name] {
			get {
				foreach (ItemDescriptor item in Items) {
					if (item.Name == name)
						return item;
				}
				return null;
			}
		}

		public static ItemGroup Empty = new ItemGroup (null, null);
	}
}
