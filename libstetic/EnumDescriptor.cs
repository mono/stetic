using System;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace Stetic {
	public class EnumValue {
		public Enum Value;
		public string Label;
		public string Description;

		internal EnumValue (Enum value, string label, string description)
		{
			Value = value;
			Label = label;
			Description = description;
		}
	}

	public class EnumDescriptor {

		Type enumType;
		Hashtable values;
		Enum[] values_array;
		string name;

		public EnumDescriptor (XmlElement elem)
		{
			string cls = elem.GetAttribute ("type");
			enumType = Registry.GetType (cls, true);
			this.name = enumType.FullName;
			
			values = new Hashtable ();

			Array enumvalues = Enum.GetValues (enumType);
			ArrayList list = new ArrayList ();
			Hashtable evalues = new Hashtable ();
			for (int i = 0; i < enumvalues.Length; i++) {
				Enum value = (Enum)Enum.ToObject (enumType, (int)enumvalues.GetValue (i));
				list.Add (value);
				evalues[Enum.GetName (enumType, value)] = value;
			}

			foreach (XmlElement valueElem in elem.SelectNodes ("value")) {
				string name = valueElem.GetAttribute ("name");
				if (!evalues.Contains (name))
					throw new ArgumentException ("<enum> node for " + enumType.FullName + " contains extra element " + name);
				Enum value = (Enum)evalues[name];
				values[value] = new EnumValue (value,
							       valueElem.GetAttribute ("label"),
							       valueElem.GetAttribute ("description"));
				evalues.Remove (name);
			}
			
			// Remove from the array the values not declared in the xml file
			foreach (object val in evalues.Values)
				list.Remove (val);

			values_array = (Enum[]) list.ToArray (typeof(Enum));
		}
		
		public string Name {
			get { return name; }
		}

		public Type EnumType {
			get {
				return enumType;
			}
		}

		public Enum[] Values {
			get {
				return values_array;
			}
		}

		public EnumValue this[Enum value] {
			get {
				return (EnumValue)values[value];
			}
		}
	}
}
