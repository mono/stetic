using System;
using System.Collections;
using System.Reflection;
using System.Xml;
using System.Xml.Xsl;

namespace Stetic {
	public static class Registry {

		static Hashtable classes_by_type = new Hashtable ();
		static Hashtable classes_by_cname = new Hashtable ();
		static Hashtable classes_by_csname = new Hashtable ();
		static Hashtable enums = new Hashtable ();

		static XslTransform gladeImport, gladeExport;

		static Registry ()
		{
			Assembly libstetic = Assembly.GetExecutingAssembly ();
			System.IO.Stream stream = libstetic.GetManifestResourceStream ("objects.xml");
			XmlDocument objects = new XmlDocument ();
			objects.Load (stream);
			stream.Close ();

			foreach (XmlElement element in objects.SelectNodes ("/objects/enum")) {
				EnumDescriptor enm = new EnumDescriptor (element);
				enums[enm.EnumType] = enm;
			}

			foreach (XmlElement element in objects.SelectNodes ("/objects/object")) {
				ClassDescriptor klass = new ClassDescriptor (libstetic, element);
				classes_by_type[klass.WrappedType] = klass;
				classes_by_cname[klass.CName] = klass;
				classes_by_csname[klass.WrappedType.FullName] = klass;
			}

			XmlDocument doc = CreateGladeTransformBase ();
			XmlNamespaceManager nsm = new XmlNamespaceManager (doc.NameTable);
			nsm.AddNamespace ("xsl", "http://www.w3.org/1999/XSL/Transform");

			foreach (XmlElement elem in objects.SelectNodes ("/objects/object/glade-transform/import/xsl:*", nsm))
				doc.FirstChild.PrependChild (doc.ImportNode (elem, true));
			gladeImport = new XslTransform ();
			gladeImport.Load (doc, null, null);

			doc = CreateGladeTransformBase ();
			foreach (XmlElement elem in objects.SelectNodes ("/objects/object/glade-transform/export/xsl:*", nsm))
				doc.FirstChild.PrependChild (doc.ImportNode (elem, true));
			gladeExport = new XslTransform ();
			gladeExport.Load (doc, null, null);
		}

		static XmlDocument CreateGladeTransformBase ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (
				"<xsl:stylesheet version='1.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
				"  <xsl:template match='@*|node()'>" +
				"    <xsl:copy>" +
				"      <xsl:apply-templates select='@*|node()' />" +
				"    </xsl:copy>" +
				"  </xsl:template>" +
				"</xsl:stylesheet>"
				);
			return doc;
		}

		public static IEnumerable AllClasses {
			get {
				return classes_by_type.Values;
			}
		}

		public static XslTransform GladeImportXsl {
			get {
				return gladeImport;
			}
		}

		public static XslTransform GladeExportXsl {
			get {
				return gladeExport;
			}
		}

		public static EnumDescriptor LookupEnum (Type type)
		{
			return (EnumDescriptor)enums[type];
		}

		public static ClassDescriptor LookupClass (Type type)
		{
			return (ClassDescriptor)classes_by_type[type];
		}

		public static ClassDescriptor LookupClass (string cname)
		{
			return (ClassDescriptor)classes_by_cname[cname];
		}
		
		static ClassDescriptor FindGroupClass (string name, out string groupname)
		{
			int sep = name.LastIndexOf ('.');
			string classname = name.Substring (0, sep);
			groupname = name.Substring (sep + 1);
			ClassDescriptor klass = (ClassDescriptor)classes_by_csname[classname];
			if (klass == null) {
				klass = (ClassDescriptor)classes_by_csname[name];
				if (klass == null)
					throw new ArgumentException ("No class for itemgroup " + name);
				classname = name;
				groupname = "";
			}
			return klass;
		}

		public static ItemGroup LookupItemGroup (string name)
		{
			string groupname;
			ClassDescriptor klass = FindGroupClass (name, out groupname);
			
			ItemGroup group = klass.ItemGroups [groupname];
			if (group != null)
				return group;
			else
				throw new ArgumentException ("No itemgroup '" + groupname + "' in class " + klass.WrappedType);
		}

		public static ItemGroup LookupSignalGroup (string name)
		{
			string groupname;
			ClassDescriptor klass = FindGroupClass (name, out groupname);
			
			ItemGroup group = klass.SignalGroups [groupname];
			if (group != null)
				return group;
			else
				throw new ArgumentException ("No itemgroup '" + groupname + "' in class " + klass.WrappedType);
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

		public static ItemGroup LookupContextMenu (string classname)
		{
			ClassDescriptor klass = (ClassDescriptor)classes_by_csname[classname];
			if (klass == null)
				throw new ArgumentException ("No class for contextmenu " + classname);
			return klass.ContextMenu;
		}

		public static object NewInstance (Type type, IProject proj)
		{
			return LookupClass (type).NewInstance (proj);
		}
	}
}
