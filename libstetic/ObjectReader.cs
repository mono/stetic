
using System;
using System.Xml;

namespace Stetic
{
	public class ObjectReader
	{
		FileFormat format;
		IProject proj;
		
		public ObjectReader (IProject proj, FileFormat format)
		{
			this.format = format;
			this.proj = proj;
		}
		
		public FileFormat Format {
			get { return format; }
		}
		
		public IProject Project {
			get { return proj; }
		}
		
		public virtual ObjectWrapper ReadObject (XmlElement elem)
		{
			return Stetic.ObjectWrapper.ReadObject (this, elem);
		}
		
		public virtual void ReadObject (ObjectWrapper wrapper, XmlElement elem)
		{
			wrapper.Read (this, elem);
		}
	}
}
