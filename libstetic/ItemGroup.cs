using System;
using System.Collections;
using System.Xml;

namespace Stetic {
	public struct ItemGroup : IEnumerable {
		public static ItemGroup Empty;

		static ItemGroup ()
		{
			Empty = new ItemGroup ();
			Empty.items = new ItemDescriptor[0];
		}

		public ItemGroup (XmlElement elem, ClassDescriptor klass)
		{
			label = elem.GetAttribute ("label");
			name = elem.GetAttribute ("name");

			XmlNodeList nodes = elem.ChildNodes;
			items = new ItemDescriptor[nodes.Count];
			for (int i = 0; i < nodes.Count; i++) {
				XmlElement item = (XmlElement)elem.ChildNodes[i];
				string refname = item.GetAttribute ("ref");
				if (refname != "") {
					if (refname.IndexOf ('.') != -1)
						items[i] = Registry.LookupItem (refname);
					else
						items[i] = klass[refname];
					continue;
				}

				if (item.Name == "property")
					items[i] = new PropertyDescriptor ((XmlElement)item, this, klass);
				else if (item.Name == "command")
					items[i] = new CommandDescriptor ((XmlElement)item, this, klass);
				else
					throw new ApplicationException ("Bad item name " + item.Name + " in " + klass.WrapperType.Name);
			}
		}

		string label, name;
		ItemDescriptor[] items;

		public string Label {
			get {
				return label;
			}
		}

		public string Name {
			get {
				return name;
			}
		}

		public IEnumerator GetEnumerator ()
		{
			return items.GetEnumerator ();
		}

		public ItemDescriptor this [string name] {
			get {
				foreach (ItemDescriptor item in items) {
					if (item != null && item.Name == name)
						return item;
				}
				return null;
			}
		}
	}
}
