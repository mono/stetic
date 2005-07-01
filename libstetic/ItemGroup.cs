using System;
using System.Reflection;
using System.Xml;

namespace Stetic {
	public struct ItemGroup {
		public string Label, Name;
		public ItemDescriptor[] Items;

		public static ItemGroup Empty;

		static ItemGroup ()
		{
			Empty = new ItemGroup ();
			Empty.Items = new ItemDescriptor[0];
		}

		public ItemGroup (XmlElement elem, ClassDescriptor klass)
		{
			Label = elem.GetAttribute ("label");
			Name = elem.GetAttribute ("name");

			XmlNodeList nodes = elem.ChildNodes;
			Items = new ItemDescriptor[nodes.Count];
			for (int i = 0; i < nodes.Count; i++) {
				XmlElement item = (XmlElement)elem.ChildNodes[i];
				string refname = item.GetAttribute ("ref");
				if (refname != "") {
					if (refname.IndexOf ('.') != -1)
						Items[i] = Registry.LookupItem (refname);
					else
						Items[i] = klass[refname];
					continue;
				}

				if (item.Name == "property")
					Items[i] = new PropertyDescriptor ((XmlElement)item, this, klass);
				else if (item.Name == "command")
					Items[i] = new CommandDescriptor ((XmlElement)item, this, klass);
				else
					throw new ApplicationException ("Bad item name " + item.Name + " in " + klass.WrapperType.Name);
			}
		}

		public ItemDescriptor this [string name] {
			get {
				foreach (ItemDescriptor item in Items) {
					if (item != null && item.Name == name)
						return item;
				}
				return null;
			}
		}
	}
}
