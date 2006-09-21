using System;
using System.Reflection;
using System.Xml;

namespace Stetic
{
	public class AssemblyWidgetLibrary: WidgetLibrary
	{
		Assembly assembly;
		DateTime timestamp;
		string name;
		
		public AssemblyWidgetLibrary (string name, Assembly assembly)
		{
			this.name = name;
			this.assembly = assembly;
			timestamp = System.IO.File.GetLastWriteTime (assembly.Location);
		}
		
		public AssemblyWidgetLibrary (string assemblyPath)
		{
			this.name = assemblyPath;
			if (assemblyPath.EndsWith (".dll") || assemblyPath.EndsWith (".exe"))
				assembly = Assembly.LoadFrom (assemblyPath);
			else
				assembly = Assembly.Load (assemblyPath);
			timestamp = System.IO.File.GetLastWriteTime (assembly.Location);
		}
		
		public override string Name {
			get { return name; }
		}
		
		public override bool NeedsReload {
			get {
				if (!System.IO.File.Exists (assembly.Location))
					return false;
				return System.IO.File.GetLastWriteTime (assembly.Location) != timestamp;
			}
		}
		
		public override bool CanReload {
			get { return false; }
		}
		
		public Assembly Assembly {
			get { return assembly; }
		}
		
		public DateTime TimeStamp {
			get { return timestamp; }
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
/*
			Disabled. The problem is that Assembly.Load tries to load the exact version
			of the assembly, and loaded references may not have the same exact version.
			
			foreach (AssemblyName an in assembly.GetReferencedAssemblies ()) {
				Assembly a = Assembly.Load (an);
				t = a.GetType (typeName);
				if (t != null) return t;
			}
*/
			return null;
		}
		
		public override System.IO.Stream GetResource (string name)
		{
			return assembly.GetManifestResourceStream (name);
		}
	}
	
}
