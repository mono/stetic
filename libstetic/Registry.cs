using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

namespace Stetic {
	public static class Registry {

		static Hashtable classes_by_type = new Hashtable ();
		static Hashtable classes_by_cname = new Hashtable ();
		static Hashtable classes_by_csname = new Hashtable ();

		static Registry ()
		{
			Assembly libstetic = Assembly.GetExecutingAssembly ();
			System.IO.Stream stream = libstetic.GetManifestResourceStream ("objects.xml");
			XmlDocument objects = new XmlDocument ();
			objects.Load (stream);
			stream.Close ();

			foreach (XmlElement element in objects.SelectNodes ("/objects/object")) {
				ClassDescriptor klass = new ClassDescriptor (libstetic, element);
				classes_by_type[klass.WrappedType] = klass;
				classes_by_cname[klass.CName] = klass;
				classes_by_csname[klass.WrappedType.FullName] = klass;
			}
		}

		public static IEnumerable AllClasses {
			get {
				return classes_by_type.Values;
			}
		}

		public static ClassDescriptor LookupClass (Type type)
		{
			return (ClassDescriptor)classes_by_type[type];
		}

		public static ClassDescriptor LookupClass (string cname)
		{
			return (ClassDescriptor)classes_by_cname[cname];
		}

		public static ItemGroup LookupItemGroup (string name)
		{
			int sep = name.LastIndexOf ('.');
			string classname = name.Substring (0, sep);
			string groupname = name.Substring (sep + 1);
			ClassDescriptor klass = (ClassDescriptor)classes_by_csname[classname];
			if (klass == null) {
				klass = (ClassDescriptor)classes_by_csname[name];
				if (klass == null)
					throw new ArgumentException ("No class for itemgroup " + name);
				classname = name;
				groupname = "";
			}

			foreach (ItemGroup group in klass.ItemGroups) {
				if (group.Name == groupname)
					return group;
			}
			throw new ArgumentException ("No itemgroup " + groupname + " in class " + classname);
		}

		public static ItemDescriptor LookupItem (string name)
		{
			int sep = name.LastIndexOf ('.');
			string classname = name.Substring (0, sep);
			string propname = name.Substring (sep + 1);
			ClassDescriptor klass = (ClassDescriptor)classes_by_csname[classname];
			if (klass == null)
				throw new ArgumentException ("No class " + classname + " for property " + propname);
			return klass[propname];
		}
	}
}
