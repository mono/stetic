using System;
using System.Collections;
using System.Xml;

namespace Stetic
{
	public abstract class WidgetLibrary: IDisposable
	{
		Hashtable classes_by_cname = new Hashtable ();
		Hashtable classes_by_csname = new Hashtable ();
		Hashtable enums = new Hashtable ();

		XmlElement[] importElems = new XmlElement [0];
		XmlElement[] exportElems = new XmlElement [0];
		
		public event EventHandler Changed;
		
		public virtual void Load ()
		{
		}

		protected virtual void Load (XmlDocument objects)
		{
			classes_by_cname.Clear ();
			classes_by_csname.Clear ();
			enums.Clear ();
			
			foreach (XmlElement element in objects.SelectNodes ("/objects/enum")) {
				EnumDescriptor enm = new EnumDescriptor (element);
				enums[enm.Name] = enm;
			}

			foreach (XmlElement element in objects.SelectNodes ("/objects/object")) {
				ClassDescriptor klass = LoadClassDescriptor (element);
				if (klass == null) continue;
				classes_by_cname[klass.CName] = klass;
				classes_by_csname[klass.WrappedTypeName] = klass;
			}

			XmlNamespaceManager nsm = new XmlNamespaceManager (objects.NameTable);
			nsm.AddNamespace ("xsl", "http://www.w3.org/1999/XSL/Transform");
			
			XmlNodeList nodes = objects.SelectNodes ("/objects/object/glade-transform/import/xsl:*", nsm);
			importElems = new XmlElement [nodes.Count];
			for (int n=0; n<nodes.Count; n++)
				importElems [n] = (XmlElement) nodes[n];
				
			nodes = objects.SelectNodes ("/objects/object/glade-transform/export/xsl:*", nsm);
			exportElems = new XmlElement [nodes.Count];
			for (int n=0; n<nodes.Count; n++)
				exportElems [n] = (XmlElement) nodes[n];
		}
		
		public virtual void Dispose ()
		{
		}
		
		protected abstract ClassDescriptor LoadClassDescriptor (XmlElement element);
		
		
		public virtual XmlElement[] GetGladeImportTransformElements ()
		{
			return importElems;
		}

		public virtual XmlElement[] GetGladeExportTransformElements ()
		{
			return exportElems;
		}

		public virtual ICollection AllClasses {
			get {
				return classes_by_csname.Values;
			}
		}

		public virtual EnumDescriptor LookupEnum (string typeName)
		{
			return (EnumDescriptor)enums[typeName];
		}

		public virtual ClassDescriptor LookupClassByCName (string cname)
		{
			return (ClassDescriptor)classes_by_cname[cname];
		}
		
		public virtual ClassDescriptor LookupClassByName (string csname)
		{
			return (ClassDescriptor)classes_by_csname[csname];
		}
		
		public virtual Type GetType (string typeName)
		{
			return null;
		}
		
		protected virtual void OnChanged ()
		{
			if (Changed != null)
				Changed (this, EventArgs.Empty);
		}
	}
}
