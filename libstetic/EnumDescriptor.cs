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

		public EnumDescriptor (XmlElement elem)
		{
			enumType = Type.GetType (elem.GetAttribute ("type"), true);
			values = new Hashtable ();

			Array enumvalues = Enum.GetValues (enumType);
			values_array = new Enum[enumvalues.Length];
			Hashtable evalues = new Hashtable ();
			for (int i = 0; i < enumvalues.Length; i++) {
				Enum value = (Enum)Enum.ToObject (enumType, (int)enumvalues.GetValue (i));
				values_array[i] = value;
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
			}

			if (values.Count != evalues.Count)
				throw new ArgumentException ("<enum> node for " + enumType.FullName + " is missing some values");
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
