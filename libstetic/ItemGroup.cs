using System;
using System.Collections;
using System.Xml;

namespace Stetic {
	public class ItemGroup : IEnumerable {
		public static ItemGroup Empty;

		static ItemGroup ()
		{
			Empty = new ItemGroup ();
		}
		
		private ItemGroup ()
		{
		}

		public ItemGroup (XmlElement elem, ClassDescriptor klass)
		{
			label = elem.GetAttribute ("label");
			name = elem.GetAttribute ("name");

			XmlNodeList nodes = elem.SelectNodes ("property | command | signal");
			for (int i = 0; i < nodes.Count; i++) {
				XmlElement item = (XmlElement)nodes[i];
				string refname = item.GetAttribute ("ref");
				if (refname != "") {
					if (refname.IndexOf ('.') != -1)
						items.Add (Registry.LookupItem (refname));
					else
						items.Add (klass[refname]);
					continue;
				}

				ItemDescriptor idesc = klass.CreateItemDescriptor ((XmlElement)item, this);
				if (idesc != null)
					items.Add (idesc);
			}
		}

		string label, name;
		ArrayList items = new ArrayList ();

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
