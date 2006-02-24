using System;
using System.Reflection;
using System.Xml;

namespace Stetic
{
	public class AssemblyWidgetLibrary: WidgetLibrary
	{
		Assembly assembly;
		
		public AssemblyWidgetLibrary (Assembly assembly)
		{
			this.assembly = assembly;
		}
		
		public AssemblyWidgetLibrary (string assemblyPath)
		{
			assembly = Assembly.LoadFrom (assemblyPath);
		}
		
		public override void Load ()
		{
			System.IO.Stream stream = assembly.GetManifestResourceStream ("objects.xml");
			if (stream == null)
				throw new InvalidOperationException ("objects.xml file not found in assembly: " + assembly.Location);
				
			XmlDocument objects = new XmlDocument ();
			objects.Load (stream);
			stream.Close ();
			Load (objects);
		}

		protected override ClassDescriptor LoadClassDescriptor (XmlElement element)
		{
			return new TypedClassDescriptor (assembly, element);
		}
		
		public override Type GetType (string typeName)
		{
			Type t = assembly.GetType (typeName, false);
			if (t != null) return t;
			
			// Look in referenced assemblies
			
			foreach (AssemblyName an in assembly.GetReferencedAssemblies ()) {
				Assembly a = Assembly.Load (an);
				t = a.GetType (typeName);
				if (t != null) return t;
			}
			return null;
		}
	}
	
}
